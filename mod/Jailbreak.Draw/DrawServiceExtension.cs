using Jailbreak.Draw.Global;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Draw;

public static class DrawServiceExtension
{
    public static void AddJailbreakDraw(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPluginBehavior<IDrawService, DrawManager>();
    }
}