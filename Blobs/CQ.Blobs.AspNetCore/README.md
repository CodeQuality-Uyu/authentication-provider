# CQ.Blobs.AspNetCore

Endpoints de blobs para ASP.NET Core sobre [CQ.Blobs](../CQ.Blobs/README.md):

| Endpoint | Cuerpo / ruta | Respuesta |
| --- | --- | --- |
| `POST /blobs` | `{ contentType, key? }` | `{ key, readUrl, writeUrl }` |
| `GET /blobs/{**key}` | la key, con sus barras | `{ key, url }` |

## Uso

El paquete trae un controller base **sin ruta ni autenticación**. Cada API lo hereda y le
pone los suyos, porque cada una se autentica con sus propios filtros de MVC:

```csharp
// Un bucket por cliente (eColors): no sobrescribe nada
[Route("blobs")]
[BearerAuthentication]
public sealed class BlobController : BlobControllerBase<CreateBlobUploadRequest>;
```

```csharp
// Varios clientes en el mismo bucket (Auth Provider)
public sealed record CreateBlobRequest : CreateBlobUploadRequest
{
    public Guid? AppId { get; init; }
}

[Route("blobs")]
[BearerAuthentication]
public sealed class BlobController : BlobControllerBase<CreateBlobRequest>
{
    // Dónde se reserva: temporary/{tenant}/{app}/…
    protected override Task<string[]> ResolveUploadScopeAsync(CreateBlobRequest request)
    {
        var account = this.GetAccountLogged();
        var app = request.AppId is null
            ? account.AppLogged
            : account.Apps.FirstOrDefault(a => a.Id == request.AppId)
              ?? throw new InvalidOperationException("The app does not belong to the logged account.");

        return Task.FromResult(new[] { account.Tenant.Name, app.Name });
    }

    // Qué puede leer y sobrescribir: todo lo de su tenant
    protected override Task<string[]> ResolveReadScopeAsync()
        => Task.FromResult(new[] { this.GetAccountLogged().Tenant.Name });
}
```

```csharp
builder.Services
    .AddBlobs(builder.Configuration)
    .AddBlobEndpoints(builder.Configuration);
```

```jsonc
{
  "Blob": {
    "Type": "aws",
    // …el resto de CQ.Blobs
    "Endpoints": {
      "AllowedContentTypes": ["image/*", "application/pdf"], // vacío = cualquiera
      "AllowClientKey": false
    }
  }
}
```

## Alcance

- **De subida** (`ResolveUploadScopeAsync`): los segmentos bajo los que se reserva la key.
- **De lectura** (`ResolveReadScopeAsync`): las keys que quien llama puede leer con `GET` y
  sobrescribir con `POST { key }`. Son las de `{alcance}/…` y `{temporary}/{alcance}/…`.
  Suele ser más amplio que el de subida (el tenant, sin la app).

Sin sobrescribirlos, no hay alcance: sirve para una API con un bucket por cliente, donde
nadie más escribe.

El alcance usa **nombres** (normalizados: `Creative Color Labs` → `creative-color-labs`),
porque así están armadas las keys que ya existen. **Si se renombra un tenant, sus keys
viejas quedan fuera del alcance nuevo**: no se pueden sobrescribir ni leer por
`GET /blobs`. Las URLs que devuelve cada recurso siguen funcionando.

## Sobrescribir una key (`AllowClientKey`)

Con `POST { contentType, key }` se obtiene una URL para pisar un archivo existente, sin
cambiar su key. **Está apagado por defecto**: con una key del cliente se puede pisar
cualquier objeto del alcance (y sin alcance, del bucket).

Existe por los fronts que reemplazan reusando la key (`keyOnReplace: 'reuse'` en el front
de licenciatario de eColors). El reemplazo seguro es reservar una key nueva y que el
recurso la promueva con `StageReplacementAsync`. Cuando no quede ningún front que reuse,
apagarlo.

Con el flag prendido, la key tiene que ser válida (sin segmentos vacíos ni `..`) y estar en
el alcance de lectura. Una key temporal del alcance también se acepta: un formulario que
cambia el archivo antes de guardar vuelve a pedir la key que reservó recién.

## Errores

El paquete **no trae filtro de excepciones**: cada API tiene su formato de error.
`BlobHttpErrors.All` lista cada excepción con su código, status y mensaje, para registrarla
en el mecanismo de cada una:

| Excepción | Código | Status |
| --- | --- | --- |
| `BlobKeyNotInTemporaryException` | `BLOB_KEY_NOT_IN_TEMPORARY` | 400 |
| `BlobContentTypeNotAllowedException` | `BLOB_CONTENT_TYPE_NOT_ALLOWED` | 400 |
| `BlobClientKeyNotAllowedException` | `BLOB_CLIENT_KEY_NOT_ALLOWED` | 400 |
| `BlobKeyOutOfScopeException` | `BLOB_KEY_OUT_OF_SCOPE` | 403 |
| `BlobNotFoundException` | `BLOB_NOT_FOUND` | 404 |
| `BlobTemporaryExpiredException` | `BLOB_TEMPORARY_EXPIRED` | 422 |

Todas son `InvalidOperationException` con el código como mensaje, así que un catálogo que
mapea por mensaje (el `ErrorCatalog` de eColors) las reconoce agregando los códigos.
