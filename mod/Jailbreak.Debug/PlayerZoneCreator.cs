using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Debug.Subcommands;
using Jailbreak.Public.Mod.Zones;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Debug;

public class PlayerZoneCreator : BasicZoneCreator, ITypedZoneCreator {
  private readonly CCSPlayerController player;
  private readonly BasePlugin plugin;
  private readonly IZoneFactory factory;
  private Timer? timer;

  public PlayerZoneCreator(BasePlugin plugin, CCSPlayerController player,
    IZoneFactory factory, ZoneType type) {
    if (!player.IsValid)
      throw new ArgumentException("Player must be valid", nameof(player));
    this.player  = player;
    this.plugin  = plugin;
    this.factory = factory;
    Type         = type;
  }

  public override void BeginCreation() {
    base.BeginCreation();
    timer = plugin.AddTimer(1f, tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void tick() {
    if (!player.IsValid) {
      timer?.Kill();
      return;
    }

    var pawn = player.PlayerPawn.Value;
    if (pawn == null) {
      timer?.Kill();
      return;
    }

    if (pawn.AbsOrigin == null) return;

    AddPoint(pawn.AbsOrigin);
    var zone = Build(factory);
    zone.Draw(plugin, Color.Gray, 1f);
  }

  public override void Dispose() {
    base.Dispose();
    timer?.Kill();
  }

  public ZoneType Type { get; }
}