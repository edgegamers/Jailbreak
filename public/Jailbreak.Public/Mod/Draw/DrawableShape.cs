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
  protected readonly BasePlugin Plugin = plugin;

  protected Timer?
    KillTimer; // Internal timer used to remove the shape after a certain amount of time

  private Vector position = position.Clone();

  // Note that this can mean different things for different shapes
  public DateTime StartTime { get; protected set; } = DateTime.Now;

  public Vector Position {
    get => position.Clone();
    protected set => position = value.Clone();
  }

  public abstract void Draw();

  public virtual void Update() {
    Remove();
    Draw();
  }

  /// <summary>
  ///   Tells the shape to draw itself and then remove itself after a certain amount of time
  /// </summary>
  /// <param name="lifetime">Time in seconds the shape should be shown</param>
  public void Draw(float lifetime) {
    Draw();
    KillTimer = Plugin.AddTimer(lifetime, Remove, TimerFlags.STOP_ON_MAPCHANGE);
  }

  public virtual void Move(Vector newPosition) { Position = newPosition; }

  public abstract void Remove();
}