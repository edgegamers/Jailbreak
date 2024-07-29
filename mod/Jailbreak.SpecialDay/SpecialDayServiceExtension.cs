using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay;

public static class SpecialDayServiceExtension {
  public static void AddJailbreakSpecialDay(this IServiceCollection collection) {
    collection.AddPluginBehavior<ISpecialDayFactory, SpecialDayFactory>();
    collection.AddPluginBehavior<ISpecialDayManager, SpecialDayManager>();
    collection.AddPluginBehavior<SpecialDayCommand>();
  }
}