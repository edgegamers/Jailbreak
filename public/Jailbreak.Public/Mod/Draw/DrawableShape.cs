using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
/// Represents a drawable shape
/// </summary>
public abstract class DrawableShape
{

    protected BasePlugin plugin;
    protected Vector position; // Represents the origin of the shape
    // Note that this can mean different things for different shapes
    protected DateTime startTime = DateTime.Now;

    private Timer? killTimer; // Internal timer used to remove the shape after a certain amount of time

    public DrawableShape(BasePlugin plugin, Vector position)
    {
        this.plugin = plugin;
        this.position = position;
    }

    public abstract void Draw();

    public virtual void Update() {
        Remove();
        Draw();
    }

    public virtual void Tick() { }

    public void Draw(float lifetime)
    {
        Draw();
        killTimer = plugin.AddTimer(lifetime, Remove, TimerFlags.STOP_ON_MAPCHANGE);
    }

    public virtual void Move(Vector position)
    {
        this.position = position;
    }

    public abstract void Remove();
}