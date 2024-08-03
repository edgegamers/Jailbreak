using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Rebel.C4Bomb;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Rebel;

public static class RebelServiceExtension {
  public static void AddJailbreakRebel(this IServiceCollection collection) {
    collection.AddPluginBehavior<IRebelService, RebelManager>();
    collection.AddPluginBehavior<IC4Service, C4Behavior>();
    collection.AddPluginBehavior<RebelListener>();
  }
}