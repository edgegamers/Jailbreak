using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;
using Jailbreak.SpecialDay.Commands;

namespace Jailbreak.SpecialDay;

public static class SpecialDayServiceExtension
{
    public static void AddSpecialDays(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddConfig<SpecialDayConfig>("SpecialDay");
        serviceCollection.AddPluginBehavior<ISpecialDayHandler, SpecialDayHandler>();
        serviceCollection.AddPluginBehavior<ISpecialDayMenu, SpecialDayMenu>();
        serviceCollection.AddPluginBehavior<SpecialDayCommandsBehavior>();
    }
}
