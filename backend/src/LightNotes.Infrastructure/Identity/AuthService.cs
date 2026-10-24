using LightNotes.Application.Abstractions;
using LightNotes.Application.Auth;
using LightNotes.Application.Common;
using LightNotes.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LightNotes.Infrastructure.Identity;

internal sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IPasswordHasher<ApplicationUser> passwordHasher,
    AppDbContext db,
    TokenService tokens,
    ICurrentUser currentUser,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider
) : IAuthService
{
    private static readonly Error InvalidCredentials = Error.Unauthorized(
        "auth.invalid_credentials"
    );
    private static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "auth.invalid_refresh_token"
    );

    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<Result> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        if (password.Length > AuthRules.PasswordMaxLength)
        {
            return Error.Validation("auth.password_invalid");
        }

        var normalizedEmail = email.Trim();
        var user = new ApplicationUser { UserName = normalizedEmail, Email = normalizedEmail };

        try
        {
            var result = await userManager.CreateAsync(user, password);

            return result.Succeeded ? Result.Success : MapIdentityErrors(result.Errors);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException
                    is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }
            )
        {
            return Error.Conflict("auth.email_taken");
        }
    }

    public async Task<Result<AuthSession>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        var user = await userManager.FindByEmailAsync(email.Trim());

        if (user is null)
        {
            _ = passwordHasher.HashPassword(new ApplicationUser(), password);

            return InvalidCredentials;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Error.RateLimited("auth.locked_out");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);

            return InvalidCredentials;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return await StartSessionAsync(user.Id, Guid.CreateVersion7(), cancellationToken);
    }

    public async Task<Result<AuthSession>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();
        var hash = TokenService.Hash(refreshToken);

        var stored = await db
            .RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= now)
        {
            return InvalidRefreshToken;
        }

        var claimed = await db
            .RefreshTokens.Where(t => t.Id == stored.Id && t.UsedAt == null && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, now), cancellationToken);

        if (claimed == 0)
        {
            await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);

            return Error.Unauthorized("auth.refresh_token_reused");
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);

            return InvalidRefreshToken;
        }

        return await StartSessionAsync(user.Id, stored.FamilyId, cancellationToken);
    }

    public async Task<Result> LogoutAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Result.Success;
        }

        var hash = TokenService.Hash(refreshToken);
        var familyId = await db
            .RefreshTokens.AsNoTracking()
            .Where(t => t.TokenHash == hash)
            .Select(t => (Guid?)t.FamilyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (familyId is not null)
        {
            await RevokeFamilyAsync(familyId.Value, timeProvider.GetUtcNow(), cancellationToken);
        }

        return Result.Success;
    }

    public async Task<Result<UserProfile>> GetProfileAsync(CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());

        if (user is null)
        {
            return Error.Unauthorized("auth.invalid_token");
        }

        return new UserProfile(user.Id, user.Email!, user.AiConsentAt is not null);
    }

    private async Task<AuthSession> StartSessionAsync(
        Guid userId,
        Guid familyId,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();
        var (accessToken, accessExpiresAt) = tokens.CreateAccessToken(userId);
        var (refreshToken, refreshHash) = TokenService.NewRefreshToken();
        var refreshExpiresAt = now + _jwt.RefreshTokenLifetime;

        db.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                FamilyId = familyId,
                TokenHash = refreshHash,
                CreatedAt = now,
                ExpiresAt = refreshExpiresAt,
            }
        );
        await db.SaveChangesAsync(cancellationToken);

        return new AuthSession(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    private Task<int> RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken
    ) =>
        db
            .RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), cancellationToken);

    private static Error MapIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var codes = errors.Select(e => e.Code).ToHashSet();

        if (codes.Overlaps(["DuplicateEmail", "DuplicateUserName"]))
        {
            return Error.Conflict("auth.email_taken");
        }

        if (codes.Overlaps(["InvalidEmail", "InvalidUserName"]))
        {
            return Error.Validation("auth.email_invalid");
        }

        return codes.Any(code => code.StartsWith("Password", StringComparison.Ordinal))
            ? Error.Validation("auth.password_invalid")
            : Error.Validation("auth.registration_failed");
    }
}
