using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///     Represents a drawable shape
/// </summary>
public abstract class DrawableShape
{
    private Timer? _killTimer; // Internal timer used to remove the shape after a certain amount of time

    protected BasePlugin Plugin;

    protected Vector Position; // Represents the origin of the shape

    // Note that this can mean different things for different shapes
    protected DateTime StartTime = DateTime.Now;

    public DrawableShape(BasePlugin plugin, Vector position)
    {
        this.Plugin = plugin;
        this.Position = position;
    }

    public abstract void Draw();

    public virtual void Update()
    {
        Remove();
        Draw();
    }

    public virtual void Tick()
    {
    }

    public void Draw(float lifetime)
    {
        Draw();
        _killTimer = Plugin.AddTimer(lifetime, Remove, TimerFlags.STOP_ON_MAPCHANGE);
    }

    public virtual void Move(Vector position)
    {
        this.Position = position;
    }

    public abstract void Remove();
}