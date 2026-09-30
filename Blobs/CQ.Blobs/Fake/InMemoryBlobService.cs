using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CQ.Blobs.Fake;

/// <summary>
/// Blobs en memoria, para desarrollo y tests. Se registra como singleton, así que lo que
/// se sube, promueve o borra se mantiene entre requests mientras viva el proceso.
/// </summary>
/// <remarks>
/// <see cref="CreateUploadForKeyAsync"/> da la subida por hecha (guarda un contenido
/// vacío), porque nadie puede hacer el PUT a la URL falsa. Así el alta y la edición
/// funcionan de punta a punta sin AWS.
/// </remarks>
public sealed class InMemoryBlobService(
    IOptions<BlobOptions> options,
    IOptions<FakeBlobOptions> fakeOptions,
    ILogger<InMemoryBlobService> logger)
    : BlobServiceBase(options.Value, logger)
{
    public const string WriteBaseUrl = "https://fake-blobs.local";

    /// <summary>
    /// Todas las keys apuntan a la misma imagen, para que las pantallas se vean en modo mock.
    /// </summary>
    public const string PlaceholderUrl = "https://placehold.co/90x90";

    private readonly ConcurrentDictionary<string, StoredBlob> _blobs = new(StringComparer.Ordinal);

    private readonly IList<FakeBlobFile> _files = fakeOptions.Value.Blobs;

    /// <summary>
    /// Las keys guardadas, para verificar en tests.
    /// </summary>
    public IReadOnlyCollection<string> Keys => _blobs.Keys.ToList();

    public bool Exists(string key) => _blobs.ContainsKey(key.TrimStart('/'));

    public override Task<BlobReadWrite> CreateUploadForKeyAsync(string key, string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        _blobs[key] = new StoredBlob([], contentType);

        return Task.FromResult(new BlobReadWrite(
            key,
            GetByKey(key).Url,
            $"{WriteBaseUrl}/{key}"));
    }

    /// <remarks>
    /// Una key que ya es una URL se devuelve tal cual, para sembrar datos con imágenes
    /// reales.
    /// </remarks>
    public override BlobRead GetByKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var url = key.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? key
            : PlaceholderUrl;

        return new BlobRead { Key = key, Url = url };
    }

    public override BlobRead GetPresignedByKey(string key, PresignedReadOptions? options = null)
        => GetByKey(key);

    protected override Task CopyAsync(string sourceKey, string destinationKey)
    {
        if (!_blobs.TryGetValue(sourceKey, out var blob))
        {
            throw new BlobNotFoundException(sourceKey);
        }

        _blobs[destinationKey] = blob;

        return Task.CompletedTask;
    }

    public override async Task UploadAsync(string key, Stream content, string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer).ConfigureAwait(false);

        _blobs[key] = new StoredBlob(buffer.ToArray(), contentType);
    }

    /// <remarks>
    /// Si la key no se subió en esta ejecución, la busca en la sección <c>FakeBlob</c>, que
    /// la asocia a un archivo local.
    /// </remarks>
    public override async Task<MemoryStream> OpenReadAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_blobs.TryGetValue(key, out var blob))
        {
            return new MemoryStream(blob.Content.ToArray());
        }

        var file = _files.FirstOrDefault(f => f.Key == key);
        if (file is null || !File.Exists(file.Path))
        {
            throw new BlobNotFoundException(key);
        }

        var content = await File.ReadAllBytesAsync(file.Path).ConfigureAwait(false);

        return new MemoryStream(content);
    }

    public override Task DeleteAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _blobs.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    private sealed record StoredBlob(byte[] Content, string ContentType);
}
