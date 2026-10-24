namespace LightNotes.Application.Auth;

public sealed record AuthSession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public sealed record UserProfile(Guid Id, string Email, bool AiConsent);
