using LightNotes.Application.Common;

namespace LightNotes.Application.Sync;

public interface ISyncService
{
    Task<Result<PushResponse>> PushAsync(PushRequest request, CancellationToken cancellationToken);

    Task<Result<PullResponse>> PullAsync(long since, int limit, CancellationToken cancellationToken);
}
