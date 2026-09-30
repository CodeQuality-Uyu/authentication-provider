# CQ.Blobs

Manejo de blobs para APIs .NET sobre S3 (AWS o LocalStack), con un fake en memoria para
desarrollo y tests.

Resuelve el flujo de siempre (el cliente sube por URL prefirmada a una carpeta temporal y el
recurso la mueve a la suya), sin los problemas que ese flujo solía traer: keys armadas con
`Replace`, temporales ajenos que se podían promover, blobs huérfanos y errores después de
haber guardado.

## Registro

```csharp
builder.Services.AddBlobs(builder.Configuration);
```

```jsonc
{
  "Blob": {
    "Type": "aws",                 // mock | localstack | aws
    "BucketName": "blobs",
    "TemporaryObject": "temporary",
    "CloudFrontDomain": "https://cdn.example.com", // vacío = URLs prefirmadas
    "PresignedUrlExpiration": "00:15:00"
  },
  "AWS": { "Profile": "", "Region": "" },          // sólo con aws
  "LocalStack": {                                  // sólo con localstack
    "ServiceUrl": "http://localhost:4566",
    "AccessToken": "test",
    "SecretToken": "test"
  },
  "FakeBlob": {                                    // sólo con mock, opcional
    "Blobs": [{ "Key": "colors.xlsx", "Path": "C:\\data\\colors.xlsx" }]
  }
}
```

**El bucket necesita una lifecycle rule sobre la carpeta temporal.** La promoción copia y no
borra el temporal: así un reintento después de un guardado fallido todavía lo encuentra.

## Alcance: ¿dividir las keys o no?

El alcance (`scope` al subir, `requiredScope` al promover) es **opcional**. Sirve para que
alguien no pueda promover la subida de otro, así que depende de quién comparte el bucket:

| La API… | Alcance | Keys |
| --- | --- | --- |
| Tiene un deploy y un bucket por cliente (eColors: uno por licenciatario) | **Ninguno.** Nadie más escribe en ese bucket, y dividir repetiría lo que ya separa el deploy | `temporary/{guid}.png` → `categories/{guid}.png` |
| Atiende a varios clientes en el mismo bucket (Auth Provider: varios tenants y apps) | **Obligatorio**, por el cliente que hace la request | `temporary/{tenant}/{app}/{guid}.png` → `{tenant}/{app}/{guid}.png` |

Si una API por cliente pasa a compartir el bucket (o tiene un panel transversal, como un
admin que sube para todos los licenciatarios), alcanza con empezar a pasar el alcance: las
keys que ya existen siguen siendo válidas.

Para autorizar una lectura o una sobrescritura, `IsInScope(key, scope…)` dice si la key
está en `{scope…}/` o en `{temporary}/{scope…}/`. Los endpoints de
[CQ.Blobs.AspNetCore](../CQ.Blobs.AspNetCore/README.md) ya lo usan.

## Subida

```csharp
// Sin alcance (un bucket por cliente): temporary/{guid}.png
var upload = await blobService.CreateUploadAsync(request.ContentType);

// Con alcance (bucket compartido): temporary/{tenant}/{app}/{guid}.png
var upload = await blobService.CreateUploadAsync(request.ContentType, tenant.Name, app.Name);
```

El cliente hace un PUT a `upload.WriteUrl` con los headers `Content-Type` (el mismo que se
reservó) y `x-amz-server-side-encryption: AES256`. Sin alguno de los dos, S3 responde 403.

## Alta: promover

```csharp
// Sin alcance
var imageKey = await blobService.PromoteAsync(args.ImageKey, "categories");

// Con alcance
var logoKey = await blobService.PromoteAsync(
    args.LogoKey,
    destinationFolder: $"{tenant.Name}/{app.Name}",
    requiredScope: [tenant.Name]);
```

Con `requiredScope`, la key tiene que estar en `temporary/{tenant}/…`: en un bucket
compartido, sin eso cualquiera podría mandar la key de una subida de otro tenant. Sin
`requiredScope`, basta con que la key sea temporal.

## Edición: reemplazo en dos fases

```csharp
var image = await blobService.StageReplacementAsync(
    args.ImageKey,          // null o igual a la actual = conservar
    category.ImageKey,
    "categories");          // con bucket compartido, sumar el requiredScope como en el alta

try
{
    await repository.UpdateAsync(id, new { args.Name, ImageKey = image.Key });
}
catch
{
    await blobService.RollbackAsync(image); // borra lo recién promovido
    throw;
}

await blobService.CommitAsync(image);       // borra la anterior
```

Para quitar el blob: `BlobReplacement.Removal(category.ImageKey)`, persistir `null` y
`CommitAsync`.

`CommitAsync` y `RollbackAsync` no propagan errores de borrado: los registran como warning.
El recurso ya quedó bien, y un objeto huérfano no justifica responder error.

## Errores

Todos heredan de `InvalidOperationException` y el mensaje es el código (`BlobErrorCodes`),
para mapearlos con el catálogo de errores de cada API.

| Excepción | Código | Cuándo |
| --- | --- | --- |
| `BlobKeyNotInTemporaryException` | `BLOB_KEY_NOT_IN_TEMPORARY` | Se quiso promover una key que no es temporal |
| `BlobKeyOutOfScopeException` | `BLOB_KEY_OUT_OF_SCOPE` | La key temporal es de otro alcance |
| `BlobTemporaryExpiredException` | `BLOB_TEMPORARY_EXPIRED` | El temporal ya no existe (lifecycle rule) |
| `BlobNotFoundException` | `BLOB_NOT_FOUND` | `OpenReadAsync` de una key que no existe |

## Lectura

- `GetByKey`: por CDN si hay `CloudFrontDomain` (no vence, sirve para mails); si no,
  prefirmada.
- `GetPresignedByKey`: siempre prefirmada, para lo privado. Con
  `PresignedReadOptions { InlineFileName, ContentType }` el navegador lo abre en su visor.

## Migrar desde los servicios propios

| Antes | Ahora |
| --- | --- |
| `CreateAsync(args)` / `CreateAsync(request, accountLogged)` | `CreateUploadAsync(contentType, scope…)`. Con key explícita, `CreateUploadForKeyAsync`, **sólo con keys que arma el servidor** |
| `MoveObjectFromTemporaryAsync(key, folder)` (eColors) | `PromoteAsync(key, folder)` |
| `MoveObjectAsync(key, oldApp, newApp)` (Auth Provider) | `PromoteAsync(key, $"{tenant}/{app}", tenant)` |
| `GetByKeyAsync(key)` | `GetByKey(key)` |
| `GetPresignedByKey(key, inlineFileName)` (asumía PDF) | `GetPresignedByKey(key, new() { InlineFileName = …, ContentType = "application/pdf" })` |
| `GetBlobStreamAsync(key)` (lanzaba `FileNotFoundException`) | `OpenReadAsync(key)` (lanza `BlobNotFoundException`) |
| `UploadObjectAsync(key, bytes, contentType)` | `UploadAsync(key, stream, contentType)` |
| `DeleteObjectAsync(key)` | `DeleteAsync(key)` |
