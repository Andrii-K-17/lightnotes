using Microsoft.AspNetCore.Identity;

namespace LightNotes.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>The last database change sequence number synchronized by the user.</summary>
    public long SyncSeq { get; set; }

    /// <summary>When the user consented to AI processing; null means no consent.</summary>
    public DateTimeOffset? AiConsentAt { get; set; }
}
