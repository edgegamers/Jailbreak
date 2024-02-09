using Jailbreak.Public.Mod.LastRequest;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public static class LastRequestServiceExtension
{
    public static void AddJailbreakLastRequest(this IServiceCollection services)
    {
        services.AddPluginBehavior<ILastRequestService, LastRequestService>();
    }
}