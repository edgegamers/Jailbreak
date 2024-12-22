using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rainbow;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Rainbow;

public static class RainbowServiceExtension {
  public static void AddJailbreakRainbow(this IServiceCollection collection) {
    collection.AddPluginBehavior<IRainbowColorizer, Rainbowizer>();
  }
}