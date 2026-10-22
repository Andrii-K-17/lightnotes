using LightNotes.Application.Common;

namespace LightNotes.Application.Auth;

public interface IAuthService
{
    Task<Result> RegisterAsync(string email, string password, CancellationToken cancellationToken);
}
