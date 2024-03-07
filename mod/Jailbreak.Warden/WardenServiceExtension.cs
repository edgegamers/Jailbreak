using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Warden.Commands;
using Jailbreak.Warden.Global;
using Jailbreak.Warden.Markers;
using Jailbreak.Warden.Paint;
using Jailbreak.Warden.Selection;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden;

public static class WardenServiceExtension
{
    public static void AddJailbreakWarden(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPluginBehavior<IWardenService, WardenBehavior>();
        serviceCollection.AddPluginBehavior<IWardenSelectionService, WardenSelectionBehavior>();
        serviceCollection.AddPluginBehavior<IWardenPeaceService, WardenPeaceBehaviour>();
        serviceCollection.AddPluginBehavior<IWardenLastGuardService, WardenLastGuardBehavior>();

        serviceCollection.AddPluginBehavior<WardenCommandsBehavior>();
        serviceCollection.AddPluginBehavior<WardenMarkerBehavior>();
        serviceCollection.AddPluginBehavior<WardenPaintBehavior>();
        serviceCollection.AddPluginBehavior<WardenPeaceCommandsBehavior>();
    }
}