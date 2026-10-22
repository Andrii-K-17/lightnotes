using LightNotes.Application.Auth;
using LightNotes.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LightNotes.Infrastructure.Identity;

internal sealed class AuthService(UserManager<ApplicationUser> userManager) : IAuthService
{
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
