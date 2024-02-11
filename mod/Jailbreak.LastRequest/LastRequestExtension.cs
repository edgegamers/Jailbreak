using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public static class LastRequestExtension
{
    public static void AddJailbreakLastRequest(this IServiceCollection collection)
    {
        collection.AddConfig<LastRequestConfig>("lastrequest");
        
        collection.AddPluginBehavior<ILastRequestManager, LastRequestManager>();
    }
}