using LightNotes.Application.Common;

namespace LightNotes.Application.Auth;

public interface IAuthService
{
    Task<Result> RegisterAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<AuthSession>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Exchanges a refresh token for a new session.
    /// </summary>
    Task<Result<AuthSession>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Ends the session the token belongs to.
    /// </summary>
    Task<Result> LogoutAsync(string? refreshToken, CancellationToken cancellationToken);

    Task<Result<UserProfile>> GetProfileAsync(CancellationToken cancellationToken);

    Task<Result<AuthSession>> SignInWithGoogleAsync(
        GoogleIdentity identity,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Records or withdraws the user's consent to send note text to the AI provider.
    /// </summary>
    Task<Result> SetAiConsentAsync(bool granted, CancellationToken cancellationToken);
}
