namespace LightNotes.Application.Auth;

public sealed record AuthSession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public sealed record UserProfile(Guid Id, string Email, bool AiConsent);

/// <summary>
/// Identity asserted by Google after a successful sign-in.
/// </summary>
public sealed record GoogleIdentity(string Subject, string Email, bool EmailVerified);
