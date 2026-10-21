using System.Security.Claims;
using LightNotes.Application.Abstractions;

namespace LightNotes.Api;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private const string SubjectClaim = "sub";

    public Guid UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(SubjectClaim), out var id)
            ? id
            : Guid.Empty;
}
