using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastGuard;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastGuard;

public static class RebelServiceExtension {
  public static void AddJailbreakLastGuard(this IServiceCollection collection) {
    collection.AddConfig<LastGuardConfig>("lastguard");
    collection.AddPluginBehavior<ILastGuardService, LastGuard>();
  }
}