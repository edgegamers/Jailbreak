using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///   Represents a drawable shape
/// </summary>
public abstract class DrawableShape(BasePlugin plugin, Vector position) {
  protected Timer?
    KillTimer; // Internal timer used to remove the shape after a certain amount of time

  protected BasePlugin Plugin = plugin;

  public Vector Position {
    get => position.Clone();
    protected set => position = value;
  }

  // Note that this can mean different things for different shapes
  protected DateTime StartTime = DateTime.Now;

  public abstract void Draw();

  public virtual void Update() {
    Remove();
    Draw();
  }

  public virtual void Tick() { }

  public void Draw(float lifetime) {
    Draw();
    KillTimer = Plugin.AddTimer(lifetime, Remove, TimerFlags.STOP_ON_MAPCHANGE);
  }

  public virtual void Move(Vector newPosition) { Position = newPosition; }

  public abstract void Remove();
}