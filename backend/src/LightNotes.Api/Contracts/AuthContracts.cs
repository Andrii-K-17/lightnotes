using System.ComponentModel.DataAnnotations;
using LightNotes.Application.Auth;

namespace LightNotes.Api.Contracts;

public sealed record RegisterRequest
{
    [Required, EmailAddress, StringLength(AuthRules.EmailMaxLength)]
    public string Email { get; init; } = "";

    [
        Required,
        StringLength(AuthRules.PasswordMaxLength, MinimumLength = AuthRules.PasswordMinLength)
    ]
    public string Password { get; init; } = "";
}

public sealed record LoginRequest
{
    [Required, EmailAddress, StringLength(AuthRules.EmailMaxLength)]
    public string Email { get; init; } = "";

    [Required, StringLength(AuthRules.PasswordMaxLength)]
    public string Password { get; init; } = "";
}

public sealed record AccessTokenResponse(string AccessToken, int ExpiresIn);
