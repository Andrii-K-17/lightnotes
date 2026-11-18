using LightNotes.Application.Ai;
using LightNotes.Application.Attachments;
using LightNotes.Application.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LightNotes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IAiService, AiService>();

        return services;
    }
}
