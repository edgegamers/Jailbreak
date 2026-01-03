using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Draw;

public static class DrawServiceExtension {
  public static void AddTextSpawner(this IServiceCollection collection) {
    collection.AddSingleton<ITextSpawner, TextSpawner>();
    collection.AddSingleton<IBeamShapeRegistry, BeamShapeRegistry>();
    collection.AddPluginBehavior<IBeamShapeFactory, BeamShapeFactory>();;
  }
}