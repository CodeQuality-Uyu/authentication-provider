namespace CQ.Blobs;

/// <summary>
/// Armado de keys. Una key son segmentos separados por <c>/</c>, sin barra inicial.
/// </summary>
/// <remarks>
/// Todo se arma por segmentos y nada con <c>Replace</c> sobre la key entera: un reemplazo
/// a ciegas también pisa las apariciones que no son la carpeta (el nombre de la app
/// dentro del del tenant, <c>temporary/</c> en medio de un nombre).
/// </remarks>
public static class BlobKey
{
    private static readonly Dictionary<string, string> KnownExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = "xlsx",
        ["application/vnd.ms-excel"] = "xls",
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = "docx",
        ["application/msword"] = "doc",
        ["text/plain"] = "txt",
    };

    /// <summary>
    /// Normaliza un segmento: minúsculas, sin espacios en los bordes, y espacios y barras
    /// como guiones. Así "Creative Color Labs" da "creative-color-labs", y un nombre con
    /// una barra no agrega un nivel de carpeta.
    /// </summary>
    public static string Slug(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);

        var slug = segment
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('/', '-')
            .Replace('\\', '-');

        if (slug.Length == 0)
        {
            throw new ArgumentException("A key segment cannot be empty.", nameof(segment));
        }

        return slug;
    }

    /// <summary>
    /// Une segmentos ya normalizados (o keys enteras) con <c>/</c>, sin barras repetidas.
    /// </summary>
    public static string Combine(params string[] parts)
        => string.Join('/', parts
            .SelectMany(p => p.Split('/', StringSplitOptions.RemoveEmptyEntries)));

    /// <summary>
    /// Normaliza una carpeta de uno o más niveles (<c>"Tenant X/App Y"</c> →
    /// <c>"tenant-x/app-y"</c>), segmento por segmento.
    /// </summary>
    public static string Folder(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            throw new ArgumentException("The folder cannot be empty.", nameof(path));
        }

        return string.Join('/', segments.Select(Slug));
    }

    /// <summary>
    /// Una key que apunta a un archivo: sin segmentos vacíos (barras repetidas o al final)
    /// ni relativos. Los <c>..</c> se rechazan aunque S3 no los resuelva, porque
    /// <see cref="System.Uri"/> sí los normaliza al armar URLs. La barra inicial se ignora.
    /// </summary>
    public static bool IsValid(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return key
            .TrimStart('/')
            .Split('/')
            .All(s => s.Length > 0 && s != "." && s != "..");
    }

    /// <summary>
    /// Último segmento de la key: el nombre del archivo.
    /// </summary>
    public static string FileName(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var trimmed = key.TrimEnd('/');
        var index = trimmed.LastIndexOf('/');

        return index < 0 ? trimmed : trimmed[(index + 1)..];
    }

    /// <summary>
    /// Extensión para un content type: <c>image/png</c> → <c>png</c>,
    /// <c>image/svg+xml</c> → <c>svg</c>, y los de Office con su extensión conocida.
    /// </summary>
    public static string Extension(string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var mediaType = contentType.Split(';')[0].Trim();
        if (KnownExtensions.TryGetValue(mediaType, out var known))
        {
            return known;
        }

        var slash = mediaType.IndexOf('/');
        if (slash <= 0 || slash == mediaType.Length - 1)
        {
            throw new ArgumentException($"Invalid content type: '{contentType}'.", nameof(contentType));
        }

        var subtype = mediaType[(slash + 1)..];
        var plus = subtype.IndexOf('+');

        return (plus > 0 ? subtype[..plus] : subtype).ToLowerInvariant();
    }
}
