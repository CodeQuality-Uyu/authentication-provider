using Microsoft.Extensions.Logging;

namespace CQ.Blobs;

/// <summary>
/// Lo que no depende del almacenamiento: armado de keys, validación de temporales y
/// reemplazo en dos fases. Cada implementación pone sólo las primitivas.
/// </summary>
public abstract class BlobServiceBase(
    BlobOptions options,
    ILogger logger)
    : IBlobService
{
    protected BlobOptions Options { get; } = options;

    protected ILogger Logger { get; } = logger;

    private string TemporaryFolder => BlobKey.Folder(Options.TemporaryObject);

    public Task<BlobReadWrite> CreateUploadAsync(string contentType, params string[] scope)
    {
        var fileName = $"{Guid.NewGuid()}.{BlobKey.Extension(contentType)}";
        var key = BlobKey.Combine([TemporaryFolder, .. scope.Select(BlobKey.Slug), fileName]);

        return CreateUploadForKeyAsync(key, contentType);
    }

    public abstract Task<BlobReadWrite> CreateUploadForKeyAsync(string key, string contentType);

    public abstract BlobRead GetByKey(string key);

    public abstract BlobRead GetPresignedByKey(string key, PresignedReadOptions? options = null);

    public bool IsTemporaryKey(string key)
        => key
            .TrimStart('/')
            .StartsWith($"{TemporaryFolder}/", StringComparison.Ordinal);

    public async Task<string> PromoteAsync(
        string temporaryKey,
        string destinationFolder,
        params string[] requiredScope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(temporaryKey);

        var sourceKey = temporaryKey.TrimStart('/');
        AssertIsPromotable(sourceKey, temporaryKey, requiredScope);

        var destinationKey = BlobKey.Combine(
            BlobKey.Folder(destinationFolder),
            BlobKey.FileName(sourceKey));

        try
        {
            await CopyAsync(sourceKey, destinationKey).ConfigureAwait(false);
        }
        catch (BlobNotFoundException ex)
        {
            throw new BlobTemporaryExpiredException(temporaryKey, ex);
        }

        return destinationKey;
    }

    private void AssertIsPromotable(
        string sourceKey,
        string originalKey,
        string[] requiredScope)
    {
        // Sin nada después de la carpeta temporal, con una barra al final (una "carpeta") o
        // con segmentos relativos, no hay un archivo que promover.
        var isFile = BlobKey.IsValid(sourceKey) && sourceKey.Contains('/');

        if (!isFile || !IsTemporaryKey(sourceKey))
        {
            throw new BlobKeyNotInTemporaryException(originalKey);
        }

        if (requiredScope.Length == 0)
        {
            return;
        }

        if (!sourceKey.StartsWith(TemporaryScopePrefix(requiredScope), StringComparison.Ordinal))
        {
            throw new BlobKeyOutOfScopeException(originalKey);
        }
    }

    public bool IsInScope(string key, params string[] scope)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (scope.Length == 0)
        {
            return true;
        }

        if (!BlobKey.IsValid(key))
        {
            return false;
        }

        var normalized = key.TrimStart('/');

        return normalized.StartsWith(ScopePrefix(scope), StringComparison.Ordinal) ||
            normalized.StartsWith(TemporaryScopePrefix(scope), StringComparison.Ordinal);
    }

    // La barra final importa: sin ella, el alcance "cq" aceptaría "cq-otro/…".
    private static string ScopePrefix(string[] scope)
        => $"{BlobKey.Combine([.. scope.Select(BlobKey.Slug)])}/";

    private string TemporaryScopePrefix(string[] scope)
        => $"{TemporaryFolder}/{ScopePrefix(scope)}";

    public async Task<BlobReplacement> StageReplacementAsync(
        string? incomingKey,
        string? currentKey,
        string destinationFolder,
        params string[] requiredScope)
    {
        // Sin key nueva en el request, el recurso conserva la que ya tenía y no se toca el
        // almacenamiento.
        if (string.IsNullOrWhiteSpace(incomingKey) || incomingKey == currentKey)
        {
            return BlobReplacement.Unchanged(currentKey);
        }

        var key = await PromoteAsync(incomingKey, destinationFolder, requiredScope)
            .ConfigureAwait(false);

        return new BlobReplacement(key, currentKey);
    }

    public Task CommitAsync(BlobReplacement replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        return replacement.HasChanged
            ? TryDeleteAsync(replacement.PreviousKey, "commit")
            : Task.CompletedTask;
    }

    public Task RollbackAsync(BlobReplacement replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        return replacement.HasChanged
            ? TryDeleteAsync(replacement.Key, "rollback")
            : Task.CompletedTask;
    }

    private async Task TryDeleteAsync(string? key, string phase)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        try
        {
            await DeleteAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(
                ex,
                "Could not delete blob {Key} on replacement {Phase}. It is now orphaned.",
                key,
                phase);
        }
    }

    /// <summary>
    /// Copia un objeto dentro del almacenamiento.
    /// </summary>
    /// <exception cref="BlobNotFoundException">El origen no existe.</exception>
    protected abstract Task CopyAsync(string sourceKey, string destinationKey);

    public abstract Task UploadAsync(string key, Stream content, string contentType);

    public abstract Task<MemoryStream> OpenReadAsync(string key);

    public abstract Task DeleteAsync(string key);
}
