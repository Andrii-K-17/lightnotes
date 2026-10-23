namespace LightNotes.Infrastructure.Identity;

/// <summary>
/// Represents a refresh token used to securely issue new access tokens and manage user sessions.
/// </summary>
internal sealed class RefreshToken
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public Guid FamilyId { get; init; }

    public byte[] TokenHash { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset? UsedAt { get; init; }

    public DateTimeOffset? RevokedAt { get; init; }
}
