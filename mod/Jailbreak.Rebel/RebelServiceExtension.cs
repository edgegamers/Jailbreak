using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Rebel.JihadC4;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Rebel;

public static class RebelServiceExtension
{
    public static void AddJailbreakRebel(this IServiceCollection collection)
    {
        collection.AddPluginBehavior<IRebelService, RebelManager>();
        collection.AddPluginBehavior<IJihadC4Service, JihadC4Behavior>();
        collection.AddPluginBehavior<RebelListener>();
    }
}