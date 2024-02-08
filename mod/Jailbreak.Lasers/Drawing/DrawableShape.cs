using System.Drawing;

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///     Represents a drawable shape
/// </summary>
public abstract class DrawableShape : IDrawable, IColorable
{
    protected Timer? KillTimer; // Internal timer used to remove the shape after a certain amount of time

    protected BasePlugin Plugin;


    // Note that this can mean different things for different shapes
    protected DateTime StartTime = DateTime.Now;

    public DrawableShape(BasePlugin plugin, Vector position)
    {
        Plugin = plugin;
        Position = position;
    }

    public Vector Position { get; protected set; }

    public void SetPosition(Vector position)
    {
        Position = position;
    }

    public Color Color { get; protected set; }

    public void SetColor(Color color)
    {
        Color = color;
    }

    public bool Drawn { get; protected set;  }

    protected abstract void DrawInternal();

    public void Draw()
    {
        if (Drawn)
            Remove();

        DrawInternal();
        Drawn = true;
    }

    protected abstract void RemoveInternal();

    public void Remove()
    {
        if (!Drawn)
            return;

        RemoveInternal();
        Drawn = false;
    }
}
