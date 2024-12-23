using Jailbreak.Public.Mod.Draw;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Draw;

public static class TextSpawnerServiceExtension {
  public static void AddTextSpawner(this IServiceCollection collection) {
    collection.AddSingleton<ITextSpawner, TextSpawner>();
  }
}