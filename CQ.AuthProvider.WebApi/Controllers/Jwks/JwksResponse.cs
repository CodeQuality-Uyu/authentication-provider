using System.Text.Json.Serialization;
using CQ.AuthProvider.BusinessLogic.Tokens;

namespace CQ.AuthProvider.WebApi.Controllers.Jwks;

/// <summary>
/// JSON Web Key Set as described by RFC 7517.
/// </summary>
public sealed record JwksResponse(
    [property: JsonPropertyName("keys")] IReadOnlyList<JwtPublicKey> Keys);
