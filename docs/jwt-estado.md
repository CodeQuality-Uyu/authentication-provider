# Estado de la migración a JWT

Branch `jwt`. Documento para retomar el trabajo más adelante: qué quedó hecho,
qué decisiones se tomaron y por qué, y qué falta.

El detalle técnico del diseño está en [jwt-sessions.md](jwt-sessions.md). Esto es
el estado del trabajo, no la documentación del sistema.

---

## Dónde quedó

**Funcionando y compilando.** `dotnet build` limpio, sin cambios de modelo
pendientes (`dotnet ef migrations has-pending-model-changes` da negativo en
Postgres).

**No se probó contra una base real ni levantando la API**: no había Docker en el
entorno donde se hizo. Todo se verificó con arneses temporales contra los
servicios reales (ver [Qué se verificó](#qué-se-verificó)). El ida y vuelta HTTP
completo y las migraciones aplicadas están **sin ejecutar**.

## La idea en una línea

El pasaje a JWT es **opt in**. El login acepta `tokenFormat`; si no se manda,
devuelve el token opaco de siempre. Nadie se entera hasta que quiere.

```jsonc
POST /sessions/credentials
{ "email": "...", "password": "...", "appId": "..." }
// -> token opaco, expiresIn: null, refreshToken: null

POST /sessions/credentials
{ "email": "...", "password": "...", "appId": "...", "tokenFormat": "Jwt" }
// -> JWT RS256 + refreshToken + expiresIn
```

## Decisiones que se tomaron

| Decisión | Alternativa descartada | Por qué |
| --- | --- | --- |
| JWT autocontenido, sin consultar la base | Híbrido: validar el JWT y además chequear la sesión en la base | El objetivo era sacar el `SELECT` con joins de cada request y que las apps validen solas. El híbrido queda documentado en `jwt-sessions.md` por si hace falta revocación inmediata |
| RS256 + JWKS | HS256 con secreto compartido | Cada app valida con la clave pública, sin compartir secretos ni llamar al provider |
| Access corto + refresh token rotativo | Un solo token de vida larga | Acota la ventana en la que un token revocado sigue sirviendo |
| Dispatcher `BearerTokenService` | Registrar los dos servicios; flag de config; esquemas distintos | Los dos declaran `"Bearer"` y `SecureAuthenticationAttribute` resuelve con `FirstOrDefault`: el segundo quedaría muerto. El dispatcher deja convivir los dos formatos de verdad |
| Default `Opaque` | Default `Jwt` | Migración evolutiva: ningún cliente cambia de comportamiento sin pedirlo |

## Qué se verificó

Con arneses temporales contra los servicios reales (ya borrados, no quedaron en
el repo):

- **Default sin pedir nada**: formato opaco, GUID, sin expiración, sin refresh, y
  el token **sí** se persiste en la fila.
- **`tokenFormat: "Jwt"`**: JWT de tres segmentos, con expiración, refresh token
  distinto de su hash, y el token **no** se persiste.
- Los dos formatos autentican de vuelta por el dispatcher y llevan su `SessionId`,
  así que el logout funciona en ambos casos.
- Un JWT alterado se rechaza y **no cae al camino opaco** (se verificó que el
  contador de consultas a la base no se mueve).
- Un token opaco desconocido devuelve `null` (401), no excepción.
- Firma verificada contra el JWKS publicado; un token de otro emisor se rechaza.
- **Binding del JSON**: campo omitido → `Opaque`; `"Jwt"` y `"jwt"` → `Jwt`;
  valor desconocido → `JsonException` (400).
- En DI quedan registrados exactamente `BearerTokenService` y
  `SubscriptionTokenService` para el esquema `Bearer`.

## Pendientes

Ninguno bloquea el merge, pero conviene decidirlos antes de exponerlo.

1. **Probar contra una base real.** Aplicar las dos migraciones y hacer el ciclo
   login → request → refresh → logout con Postgres y con SQL Server.
2. **Los endpoints que crean cuenta siguen emitiendo tokens opacos.**
   `POST /accounts/...` y aceptar invitación devuelven sesión pero no exponen
   `tokenFormat`, así que caen en el default. Para habilitarlos: sumar el
   parámetro al args correspondiente y pasarlo a
   `ISessionInternalService.CreateAsync`, que ya lo recibe.
3. **No hay tope absoluto de sesión.** El refresh es deslizante: cada renovación
   suma otros `RefreshTokenExpirationInDays` desde ese momento, así que un
   usuario activo no se desloguea nunca. Si hace falta un corte duro, hay que
   comparar contra el `CreatedAt` de la fila en `SessionService.RefreshAsync`.
4. **Rotación de claves.** Hoy el JWKS publica una sola clave. Rotar implica
   aceptar dos a la vez durante la transición (publicar ambas, firmar con la
   nueva). No está implementado.
5. **Interceptor del lado cliente.** Ni
   [auth-provider-react-web](../../auth-provider-react-web) ni el
   [SDK](../../authentication-provider-sdk) fueron tocados. El flujo esperado y
   sus trampas (rotación, refresh concurrente, no reintentar en loop) están al
   final de este documento.
6. **Índice sobre `Sessions.Token`.** El camino opaco consulta esa columna sin
   índice, igual que antes de este cambio. Mientras haya tokens opacos vivos es
   un `SELECT` por request autenticado.

## Un hallazgo lateral

Se había registrado `JsonStringEnumConverter` global en `Program.cs` para que
`tokenFormat` viajara como nombre. Eso alcanzaba también a
`ErrorResponse.StatusCode`, que es un enum `HttpStatusCode`, y cambiaba **toda**
respuesta de error de la API:

```
sin converter global: {"statusCode":401,"code":"AuthorizationExpired",...}
con converter global: {"statusCode":"Unauthorized","code":"AuthorizationExpired",...}
```

Se sacó el registro global y se dejó `[JsonConverter]` sobre el enum
`TokenFormat`. `tokenFormat` sigue saliendo como `"Jwt"` y los errores vuelven a
devolver el número. `TokenFormat` es el único enum que viaja en una response, así
que no se pierde nada.

---

## Mapa de archivos

### Nuevos

| Archivo | Qué hace |
| --- | --- |
| `Tokens/JwtTokenService.cs` | Firma y valida el JWT; reconstruye el `AccountLogged` desde los claims |
| `Tokens/BearerTokenService.cs` | Dispatcher del esquema `Bearer`. Al emitir decide por el formato pedido, al validar por el formato del token |
| `Tokens/TokenFormat.cs` | El enum `Opaque` / `Jwt` y `TokenFormats.Of`, que lo deduce de un token suelto |
| `Tokens/RsaJwtKeyProvider.cs`, `IJwtKeyProvider.cs` | Carga la clave RSA, deriva el `kid`, expone la pública |
| `Tokens/JwtClaims.cs`, `AccessTokenPayload.cs`, `SessionTokenPayload.cs`, `JwtPublicKey.cs` | Nombres de claims y contratos de datos |
| `AppConfig/JwtSection.cs` | La configuración (ver abajo) |
| `Sessions/RefreshTokenFactory.cs` | Genera el refresh token (256 bits) y su hash SHA-256 |
| `Sessions/RefreshSessionArgs.cs`, `RefreshSessionArgsValidator.cs`, `Exceptions/InvalidRefreshTokenException.cs` | El endpoint de refresh |
| `WebApi/Controllers/Jwks/` | `GET /.well-known/jwks.json` |
| `Migrations/*_AddRefreshTokenToSession.cs` (Postgres y Sql) | Migración aditiva |

### Modificados que importan

| Archivo | Cambio |
| --- | --- |
| `Sessions/SessionService.cs` | Se bifurca en `BuildJwtSession` / `BuildOpaqueSession` |
| `Sessions/Session.cs` | Suma `TokenFormat`, `TokenExpiresAt` nullable, campos de refresh |
| `Sessions/CreateSessionCredentialsArgs.cs` | El parámetro `TokenFormat`, default `Opaque` |
| `Tokens/GuidTokenService.cs` | Sigue vivo. Ahora devuelve `null` en vez de tirar excepción cuando el token no existe |
| `DataAccess.EfCore/Sessions/SessionEfCore.cs` | Persiste el token opaco o el hash del refresh, según el formato |
| `DataAccess.EfCore/AuthDbContext.cs` | Índice **no** único sobre `RefreshTokenHash` |
| `WebApi/Program.cs` | Se sacó el `JsonStringEnumConverter` global |
| `WebApi/Controllers/Sessions/SessionController.cs` | Suma `POST /sessions/refresh`; `DELETE /sessions` ahora lleva `[BearerAuthentication]` |

### Un cambio de comportamiento fuera del alcance original

`DELETE /sessions` no tenía `[BearerAuthentication]`: sin ese filtro nadie
poblaba el `AccountLogged` del contexto y el endpoint respondía 401 siempre. Se
agregó porque el logout hacía falta para probar el ciclo completo.

---

## Configuración

```jsonc
"Jwt": {
  "Issuer": "cq-auth-provider",
  "PrivateKeyPem": "",              // RSA PKCS#8 PEM, crudo o en base64
  "KeyId": "",                      // vacío: se deriva de la clave pública
  "AccessTokenExpirationInMinutes": 15,
  "RefreshTokenExpirationInDays": 30,
  "ClockSkewInSeconds": 30
}
```

Todas aplican **solo al camino JWT**: el token opaco no se firma ni expira.

Cualquiera se pisa por variable de entorno con doble guión bajo
(`Jwt__AccessTokenExpirationInMinutes=5`), que es como corresponde manejar
`PrivateKeyPem`.

### Qué es cada una

**`Issuer`** — va al claim `iss` y se valida al recibir. Las apps consumidoras lo
tienen hardcodeado, así que cambiarlo invalida de golpe todos los tokens en
circulación. Conviene que difiera por ambiente para que un token de staging no
sea aceptado en producción.

**`PrivateKeyPem`** — la clave privada RSA. **Es el único secreto real de la
sección**: quien la tenga puede firmar un token para cualquier cuenta con
cualquier permiso. No va en el archivo versionado. Acepta el PEM crudo o en
base64, lo segundo porque los saltos de línea no sobreviven bien a una variable
de entorno. Vacía genera una clave efímera al arrancar; el arranque en
`Production` se rechaza si está vacía, porque cada instancia firmaría distinto y
cada deploy tiraría abajo todas las sesiones.

**`KeyId`** — el `kid` del header y del JWKS. Vacío se deriva del thumbprint de
la clave pública y queda estable entre reinicios e instancias, que es lo que
querés casi siempre. Setealo a mano solo si rotás claves y preferís nombrarlas.

**`AccessTokenExpirationInMinutes`** — en realidad configura **la ventana de
revocación**: como el JWT no se consulta contra la base, el logout no lo mata, y
este número es cuánto sigue sirviendo un token de una sesión ya revocada.

**`RefreshTokenExpirationInDays`** — **deslizante, no absoluto**: cada refresh lo
rota y le da otros N días. O sea, N días *de inactividad* antes de tener que
reloguearse, no un tope de sesión.

**`ClockSkewInSeconds`** — tolerancia al validar `exp` y `nbf`. Suma a la vida
efectiva: 15 minutos con 30 segundos de skew se acepta hasta 15:30. En 0 arriesga
401 espurios por deriva de relojes; la librería de Microsoft trae 5 minutos por
defecto, demasiado para un token de 15.

### Configs posibles

#### Local / Development

```jsonc
"Jwt": {
  "Issuer": "cq-auth-provider-local",
  "PrivateKeyPem": "",
  "KeyId": "",
  // 8 horas: en desarrollo no querés refrescar cada 15 minutos ni que se venza
  // el token pegado en el app.http a mitad de una prueba.
  "AccessTokenExpirationInMinutes": 480,
  "RefreshTokenExpirationInDays": 30,
  // Holgado por el desfasaje de reloj de contenedores y VMs suspendidas.
  "ClockSkewInSeconds": 300
}
```

Con la clave vacía los tokens mueren en cada reinicio, que con hot reload es
seguido. Si molesta, generá una y ponela en user-secrets — **no** en el repo:

```bash
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out jwt-dev.pem
dotnet user-secrets set "Jwt:PrivateKeyPem" "$(cat jwt-dev.pem)" --project CQ.AuthProvider.WebApi
```

#### Docker / Staging

```yaml
environment:
  Jwt__Issuer: "cq-auth-provider-staging"
  Jwt__PrivateKeyPem: "${JWT_PRIVATE_KEY_B64}"
  Jwt__AccessTokenExpirationInMinutes: 15
  Jwt__RefreshTokenExpirationInDays: 7
  Jwt__ClockSkewInSeconds: 60
```

```bash
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out jwt-staging.pem
export JWT_PRIVATE_KEY_B64=$(base64 -i jwt-staging.pem)
```

#### Producción

```bash
Jwt__Issuer=cq-auth-provider
Jwt__PrivateKeyPem=<PEM en base64, desde Secrets Manager / SSM>
Jwt__AccessTokenExpirationInMinutes=15
Jwt__RefreshTokenExpirationInDays=30
Jwt__ClockSkewInSeconds=30
```

`KeyId` sin setear: se deriva del thumbprint y queda estable entre instancias.

#### Los dos números que se tocan de verdad

El resto es infraestructura; estos dos son decisión de producto:

| Perfil | Access (min) | Refresh (días) | Por qué |
| --- | --- | --- | --- |
| Web SPA | 15 | 30 | Default razonable |
| Mobile | 15 | 90 | Nadie quiere reloguearse en el celular |
| Backoffice / admin | 5–10 | 1–7 | Más permisos, menos ventana de revocación |
| Datos sensibles (pagos, salud) | 5 | 1–7 | Compliance suele pedir sesiones cortas |
| Interno / red cerrada | 60 | 30 | Menos superficie, menos ruido de refresh |

---

## Flujo esperado del lado cliente

Para cuando se implemente el interceptor (pendiente 5).

```
POST /sessions/credentials  { ..., "tokenFormat": "Jwt" }
  -> { token, expiresIn: 900, refreshToken, ... }
     [guardar los dos; agendar refresh en ~840s]

GET /me  Authorization: Bearer eyJ...     (vencido)
  -> 401 { "statusCode": 401, "code": "AuthorizationExpired", ... }

POST /sessions/refresh  { "refreshToken": "..." }   (sin header Authorization)
  -> { token, expiresIn: 900, refreshToken }        <- refreshToken NUEVO

GET /me  Authorization: Bearer eyJ...     (reintento)
  -> 200
```

Si el refresh falla devuelve 401 con `code: "InvalidRefreshToken"`. Es terminal:
al login.

### Las tres trampas

**La rotación.** Cada refresh invalida el anterior. Si el cliente pierde el nuevo
`refreshToken`, la sesión queda muerta. Guardalo antes de dar el refresh por
exitoso.

**El refresh concurrente.** Si cinco requests reciben 401 a la vez y los cinco
llaman a refresh con el mismo token, el primero rota y los otros cuatro reciben
`InvalidRefreshToken` — y el usuario se desloguea sin motivo. Hay que serializar:
un único refresh en vuelo que los demás esperan.

```js
let refreshing = null;

function refresh() {
  refreshing ??= post('/sessions/refresh', { refreshToken: store.refreshToken })
    .then(r => { store.save(r.token, r.refreshToken); return r.token; })
    .finally(() => { refreshing = null; });

  return refreshing;
}

async function request(config) {
  let res = await send(config);

  if (res.status === 401 && !config.__retried) {
    try {
      config.headers.Authorization = await refresh();
    } catch {
      return logout();
    }
    config.__retried = true;      // una sola vez, nunca en loop
    res = await send(config);
  }

  return res;
}
```

**El 401 no distingue el motivo.** `AuthorizationExpired` es el código para
cualquier token que no resuelva: vencido, firma inválida o inexistente. Intentá
el refresh una vez y si falla, logout.

### Con token opaco

Si `tokenFormat` viene `"Opaque"` no hay `refreshToken` ni `expiresIn`: esos
tokens no vencen. El cliente no debe intentar refrescar; ante un 401 va directo
al login, y ese login nuevo ya puede pedir `"Jwt"`.

### Leer los claims

Un JWT firmado **no está encriptado**: el payload va en base64url, en claro.
Leerlo no requiere ninguna clave (`atob` del segmento del medio). Por eso **nada
secreto puede ir en el token**.

Validarlo sí requiere la clave pública del JWKS, y solo lo necesita quien recibe
el token de un tercero — una app que valida lo que le manda un cliente. El
frontend que hizo el login no valida nada: el token se lo dio el provider.
