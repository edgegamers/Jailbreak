namespace Jailbreak.Draw;

public static class DrawServiceExtension
{
    public static void AddJailbreakDraw(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddPluginBehavior<IDrawService, DrawManager>();
    }
}