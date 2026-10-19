namespace LightNotes.Application.Common;

public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    TooLarge,
    RateLimited,
    Unavailable,
}
