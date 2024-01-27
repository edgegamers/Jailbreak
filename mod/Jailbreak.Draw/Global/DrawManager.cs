using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Draw.Global;

public class DrawManager : IPluginBehavior, IDrawService
{
    private List<DrawableShape> shapes = new List<DrawableShape>();

    public DrawManager(BasePlugin plugin)
    {
        plugin.AddTimer(1f, Tick, TimerFlags.REPEAT);
    }

    private void Tick()
    {
        shapes.ForEach(s => s.Tick());
    }

    public void DrawShape(DrawableShape shape, float tickRate = 0f)
    {
    }

    public List<DrawableShape> GetShapes()
    {
        return shapes;
    }
}