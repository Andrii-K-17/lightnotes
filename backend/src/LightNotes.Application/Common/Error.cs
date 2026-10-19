namespace LightNotes.Application.Common;

public sealed record Error(ErrorType Type, string Code)
{
    public static Error Validation(string code) => new(ErrorType.Validation, code);

    public static Error Unauthorized(string code) => new(ErrorType.Unauthorized, code);

    public static Error Forbidden(string code) => new(ErrorType.Forbidden, code);

    public static Error NotFound(string code) => new(ErrorType.NotFound, code);

    public static Error Conflict(string code) => new(ErrorType.Conflict, code);

    public static Error TooLarge(string code) => new(ErrorType.TooLarge, code);

    public static Error RateLimited(string code) => new(ErrorType.RateLimited, code);

    public static Error Unavailable(string code) => new(ErrorType.Unavailable, code);
}
