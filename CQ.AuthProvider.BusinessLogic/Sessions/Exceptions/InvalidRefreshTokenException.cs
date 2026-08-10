namespace CQ.AuthProvider.BusinessLogic.Sessions.Exceptions;

/// <summary>
/// The refresh token does not exist, already expired or was already rotated.
/// </summary>
public sealed class InvalidRefreshTokenException
    : Exception
{
}
