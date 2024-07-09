using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Warden.Commands;
using Jailbreak.Warden.Global;
using Jailbreak.Warden.Markers;
using Jailbreak.Warden.Paint;
using Jailbreak.Warden.Selection;
using Jailbreak.Warden.SpecialTreatment;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden;

public static class WardenServiceExtension {
  public static void AddJailbreakWarden(
    this IServiceCollection serviceCollection) {
    serviceCollection.AddConfig<WardenConfig>("warden");
    serviceCollection.AddPluginBehavior<IWardenService, WardenBehavior>();
    serviceCollection
     .AddPluginBehavior<IWardenSelectionService, WardenSelectionBehavior>();
    serviceCollection
     .AddPluginBehavior<ISpecialTreatmentService, SpecialTreatmentBehavior>();

    serviceCollection.AddPluginBehavior<SpecialTreatmentCommandsBehavior>();
    serviceCollection.AddPluginBehavior<PeaceCommandsBehavior>();
    serviceCollection.AddPluginBehavior<WardenCommandsBehavior>();
    serviceCollection.AddPluginBehavior<RollCommandBehavior>();

    serviceCollection.AddPluginBehavior<WardenMarkerBehavior>();
    serviceCollection.AddPluginBehavior<WardenPaintBehavior>();
  }
}