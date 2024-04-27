using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay;

public static class SpecialDayServiceExtension
{
    public static void AddSpecialDays(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPluginBehavior<ISpecialDayHandler, SpecialDayHandler>();
    }
}