using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Rebel.Bomb;
using Jailbreak.Rebel.JihadC4;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Rebel;

public static class RebelServiceExtension
{
    public static void AddJailbreakRebel(this IServiceCollection collection)
    {
        collection.AddPluginBehavior<IRebelService, RebelManager>();
        collection.AddPluginBehavior<IBombService, BombBehavior>();
        collection.AddPluginBehavior<BombNotificationsBehavior>();
        collection.AddPluginBehavior<BombRandomGrantBehavior>();

        collection.AddRedirect<BombNotificationsBehavior, IBombResultHook>();

        collection.AddPluginBehavior<RebelListener>();
    }
}
