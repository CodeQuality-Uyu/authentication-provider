# Sesiones y tokens

> Estado del trabajo, decisiones tomadas y pendientes: [jwt-estado.md](jwt-estado.md).

El pasaje a JWT es **evolutivo y opt in**. El login acepta un campo
`tokenFormat`; si no se manda, se devuelve el token opaco de siempre. Nada
cambia para un cliente que no se entere de que esto existe.

```jsonc
POST /sessions/credentials
{ "email": "...", "password": "...", "appId": "..." }
// -> token opaco, como siempre

POST /sessions/credentials
{ "email": "...", "password": "...", "appId": "...", "tokenFormat": "Jwt" }
// -> JWT + refresh token
```

| | `Opaque` (default) | `Jwt` (opt in) |
| --- | --- | --- |
| Formato | guid | JWT RS256 autocontenido |
| Validación | fila en `Sessions` | firma contra el JWKS, sin base |
| Expira | no | sí, `expiresIn` |
| `refreshToken` | no | sí, rotativo |
| El cliente lo puede leer | no | sí |
| Revocación | inmediata (`DELETE /sessions`) | del refresh; el access vive hasta expirar |

Los tokens opacos emitidos antes del cambio **siguen funcionando** sin tocar
nada. Los dos formatos conviven bajo el mismo esquema `Bearer` (ver
[Convivencia con los tokens opacos](#convivencia-con-los-tokens-opacos)).

## Flujo

```
POST /sessions/credentials       -> { token, tokenFormat, expiresIn?, refreshToken?, ... }
POST /sessions/refresh           -> { token, tokenFormat, refreshToken (rotado), expiresIn, ... }
DELETE /sessions                 -> revoca la sesión actual
GET  /.well-known/jwks.json      -> clave pública para validar el token
```

`expiresIn` y `refreshToken` vienen en `null` cuando el token es opaco: no
expira y no hay nada que refrescar.

Lo que sigue describe el camino JWT.

1. **Login.** `SessionService.CreateAsync` valida credenciales contra el identity
   provider, crea una fila en `Sessions` con el hash del refresh token, y firma
   el access token con los datos de la cuenta.
2. **Cada request.** `JwtTokenService.GetOrDefaultAsync` valida firma, emisor y
   expiración, y reconstruye el `AccountLogged` desde los claims. Sin consulta a
   la base.
3. **Refresh.** Se busca la sesión por el hash del refresh token, se relee la
   cuenta desde la base (así los cambios de roles y permisos se aplican en el
   próximo refresh, no recién en el próximo login), se emite un access token
   nuevo y **se rota el refresh token**: el anterior deja de servir.
4. **Logout.** Se borra la fila de la sesión por el claim `sid`.

## El token

Firmado con RS256. Payload:

```json
{
  "iss": "cq-auth-provider",
  "aud": "<id de la app en la que se logueó>",
  "sub": "<id de la cuenta>",
  "jti": "<id del token>",
  "sid": "<id de la sesión, para revocar>",
  "iat": 1753800000, "nbf": 1753800000, "exp": 1753800900,

  "email": "...", "given_name": "...", "family_name": "...", "name": "...",
  "locale": "es-AR", "zoneinfo": "America/Argentina/Buenos_Aires",
  "picture_key": "pics/daniel.png",

  "tenant": { "id": "...", "name": "...", "mini_logo_key": "...", "cover_logo_key": "...", "web_url": "..." },
  "app":    { "id": "...", "name": "..." },
  "apps":   [ { "id": "...", "name": "..." } ],
  "roles":  [ { "id": "...", "name": "...", "key": "...", "app_id": "...",
                "permissions": [ { "id": "...", "key": "createrole" } ] } ]
}
```

Los claims vacíos se omiten. Una cuenta con un rol y un permiso da un token de
~1.7 KB; el tamaño crece con la cantidad de roles y permisos, así que conviene
tener presente el límite de header del reverse proxy (habitualmente 8 KB) en
cuentas con muchísimos permisos.

`aud` es el id de la app. Cada app debería verificar que coincida con la suya.

### Validar el token desde otra app

```
GET https://<auth-provider>/.well-known/jwks.json
```

Devuelve un JWKS (RFC 7517) con la clave pública. Validar: firma RS256 contra
esa clave, `iss` igual al configurado, `aud` igual al id de la app, y `exp`.
Ya no hace falta llamar a `POST /sessions/check`, que sigue existiendo para
quien prefiera delegar la validación.

## Configuración

```jsonc
"Jwt": {
  "Issuer": "cq-auth-provider",
  "PrivateKeyPem": "",              // RSA PKCS#8 PEM, crudo o en base64
  "KeyId": "",                      // si está vacío se deriva de la clave pública
  "AccessTokenExpirationInMinutes": 15,
  "RefreshTokenExpirationInDays": 30,
  "ClockSkewInSeconds": 30
}
```

Generar el par de claves:

```bash
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out jwt-private.pem
# para pasarlo por variable de entorno sin pelearse con los saltos de línea:
base64 -i jwt-private.pem
```

Con `PrivateKeyPem` vacío se genera una clave efímera al arrancar. Sirve en
local; en producción se rechaza el arranque, porque cada instancia firmaría con
una clave distinta y todos los tokens morirían en cada deploy.

El `kid` se deriva del thumbprint de la clave pública, así que es estable entre
instancias y reinicios mientras la clave no cambie.

## Convivencia con los tokens opacos

`JwtTokenService` y `GuidTokenService` declaran los dos el esquema `"Bearer"`, y
`SecureAuthenticationAttribute` resuelve el esquema con un `FirstOrDefault`. Si
se registraran los dos como `ITokenService`, el segundo nunca se usaría.

Por eso el único registrado para `"Bearer"` es `BearerTokenService`, que decide
por el formato del token y delega:

```csharp
// BearerTokenService
public async Task<object?> GetOrDefaultAsync(string value)
{
    var isJwt = await jwtTokenService.IsValidAsync(value);   // 3 segmentos base64url

    var tokenService = isJwt
        ? (ITokenService)jwtTokenService
        : guidTokenService;                                   // guid pelado

    return await tokenService.GetOrDefaultAsync(value);
}
```

Los formatos no se solapan —un JWT tiene tres segmentos, un token opaco es un
guid— así que la discriminación es inequívoca al validar.

Al **emitir** no hay nada que adivinar: el formato lo pide el llamador, y el
default lo mantiene todo como estaba.

```csharp
// BearerTokenService
public Task<string> CreateAsync(object item)
{
    var format = item is SessionTokenPayload payload
        ? payload.Format
        : TokenFormat.Opaque;      // el default es el de siempre

    var tokenService = format == TokenFormat.Jwt
        ? (ITokenService)jwtTokenService
        : guidTokenService;

    return tokenService.CreateAsync(item);
}
```

El default vive en tres lugares, todos apuntando a `Opaque`: el parámetro de
`CreateSessionCredentialsArgs`, el de `ISessionInternalService.CreateAsync`, y
el de `SessionTokenPayload`. Es redundante a propósito — que ninguna ruta pueda
empezar a emitir JWT por olvido.

Los dos servicios se registran como tipos concretos (`AddTransient<JwtTokenService>()`,
`AddTransient<GuidTokenService>()`) y solo el dispatcher va por `AddTokenService`.

### Qué implica en la base

La tabla `Sessions` sostiene las dos formas, nunca las dos a la vez en la misma
fila:

| Columna | Sesión opaca | Sesión JWT |
| --- | --- | --- |
| `Token` | el token opaco | `NULL` |
| `RefreshTokenHash` | `NULL` | SHA-256 del refresh token |
| `RefreshTokenExpiresAt` | `NULL` | vencimiento del refresh |

`SessionEfCore` decide por `Session.TokenFormat`, y al leer se infiere al revés:
si hay `Token` guardado la sesión es opaca, si no es JWT.

La migración `AddRefreshTokenToSession` es puramente aditiva: hace `Token`
nullable y agrega las dos columnas nuevas. **No borra sesiones**, así que nadie
pierde la sesión en el deploy.

El índice sobre `RefreshTokenHash` **no** es único, porque la columna es nula en
las sesiones opacas y SQL Server admite un solo `NULL` en un índice único. La
unicidad la da que el token son 256 bits de aleatoriedad, no el esquema.

### Cómo terminar la migración

1. Los clientes empiezan a mandar `"tokenFormat": "Jwt"` de a uno, a su ritmo.
2. Cuando no queden clientes pidiendo el formato opaco, invertir el default en
   `CreateSessionCredentialsArgs`.
3. Cuando se vacíen las filas con `Token` no nulo, se puede borrar
   `GuidTokenService`, el dispatcher, el campo `tokenFormat` y la columna
   `Token` en una migración de limpieza.

## Qué se ganó y qué se resignó

**A favor:** validar un request ya no consulta la base (antes era un `SELECT`
con varios joins por request autenticado), y las apps pueden validar el token
sin llamar al auth provider.

**En contra — el access token no se puede revocar.** El logout borra el refresh
token, pero el access token que el cliente ya tiene sigue siendo válido hasta
que expira. Por eso dura 15 minutos. Si esa ventana es inaceptable, hay dos
salidas: bajar `AccessTokenExpirationInMinutes`, o pasar al esquema híbrido.

### Alternativa: esquema híbrido

Mismo JWT, pero `GetOrDefaultAsync` verifica además que la sesión siga viva
antes de aceptar el token:

```csharp
var result = await _handler.ValidateTokenAsync(value, BuildValidationParameters());
if (!result.IsValid) return null;

var payload = ReadPayload((JsonWebToken)result.SecurityToken);

// El paso extra: la sesión tiene que seguir existiendo.
var exists = await sessionRepository.ExistsAsync(payload.SessionId);
if (!exists) return null;

return BuildAccountLogged(payload, value);
```

Con eso el logout es inmediato y se pueden revocar sesiones a mano, a cambio de
un `SELECT` por id (barato, sin joins, cacheable) en cada request, y de que las
apps ya no puedan validar el token por su cuenta: vuelven a depender de
`POST /sessions/check`.

Se eligió el autocontenido porque el objetivo era justamente que las apps
validen sin round-trip. Si más adelante pesa más la revocación inmediata, el
cambio se acota a `JwtTokenService.GetOrDefaultAsync` más un método de
existencia en `ISessionRepository`.

## Cambios respecto del esquema anterior

- **Nada cambia para quien no pida nada.** Sin `tokenFormat`, el login devuelve
  el mismo token opaco de siempre, sin expiración y sin refresh. Los tokens
  opacos ya emitidos siguen validando.
- `POST /sessions/credentials` acepta `tokenFormat`: `"Opaque"` (default) o
  `"Jwt"`. Un valor desconocido es un 400.
- Pedir `"Jwt"` es aceptar que el token expira: ese cliente tiene que manejar el
  401 y llamar a `POST /sessions/refresh`.
- `SessionCreatedResponse` suma `tokenFormat`, `refreshToken` y `expiresIn`.
  Los dos últimos vienen en `null` cuando el token es opaco. En `GET /me`
  `tokenFormat` sale del token con el que llegó el request (`TokenFormats.Of`,
  la misma regla con la que despacha `BearerTokenService`); en el resto sale de
  `Session.TokenFormat`, que es lo que efectivamente se emitió.
- `TokenFormat` viaja como nombre por el `[JsonConverter]` declarado sobre el
  enum. **No** se registra `JsonStringEnumConverter` global: eso alcanzaría
  también a `ErrorResponse.StatusCode` y convertiría el `"statusCode": 401` de
  toda respuesta de error en `"Unauthorized"`.
- Los endpoints que crean cuenta y devuelven sesión (`POST /accounts/...`,
  aceptar invitación) siguen emitiendo tokens opacos: todavía no exponen
  `tokenFormat`. Agregarlo es sumar el parámetro al args correspondiente y
  pasarlo a `ISessionInternalService.CreateAsync`.
- La tabla `Sessions` suma `RefreshTokenHash` (SHA-256 hex) y
  `RefreshTokenExpiresAt`, y `Token` pasa a nullable. Ver
  [Qué implica en la base](#qué-implica-en-la-base).
- `POST /sessions/check` devuelve el `AccountLogged` reconstruido desde los
  claims. Los logos y backgrounds de las apps del listado `apps` ya no viajan
  (no entran en el token); el tenant sí conserva sus claves de logo, que son las
  que `GET /me` expone.
- `DELETE /sessions` ahora lleva `[BearerAuthentication]`. Sin ese filtro nadie
  poblaba el `AccountLogged` del contexto y el endpoint respondía 401 siempre.
