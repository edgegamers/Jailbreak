using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Debug.Subcommands;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Debug;

public class PlayerZoneCreator : BasicZoneCreator, ITypedZoneCreator {
  private readonly IZoneFactory factory;
  private readonly CCSPlayerController player;
  private readonly BasePlugin plugin;
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
    timer = plugin.AddTimer(0.5f, tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
  }

  public override void Dispose() {
    base.Dispose();
    timer?.Kill();
  }

  public ZoneType Type { get; }

  public override IZone Build(IZoneFactory providedFactory) {
    Points = ConvexHullUtil.ComputeConvexHull(Points ?? []).ToList();
    return base.Build(providedFactory);
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
    player.PrintToChat("Adding point... " + Points?.Count);

    AddPoint(pawn.AbsOrigin.Clone());
    var zone = Build(factory);
    zone.Draw(plugin, Type.GetColor(), 1f);
  }
}