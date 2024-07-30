using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastGuard;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.SpecialDay;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Mod.SpecialDay;

public abstract class AbstractSpecialDay {
  private readonly Dictionary<string, object?> previousConvarValues = new();
  private readonly IServiceProvider provider;
  protected BasePlugin Plugin;
  protected Dictionary<float, Action> Timers = new();

  public AbstractSpecialDay(BasePlugin plugin, IServiceProvider provider) {
    Plugin        = plugin;
    this.provider = provider;
  }

  public abstract SDType Type { get; }
  public virtual SpecialDaySettings? Settings => null;

  /// <summary>
  ///   Called when the warden initially picks the special day.
  ///   Use for teleporting, stripping weapons, starting timers, etc.
  /// </summary>
  public virtual void Setup() {
    if (Settings == null) return;

    Plugin.RegisterEventHandler<EventRoundEnd>(OnEnd);

    foreach (var entry in Settings.ConVarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null) Server.PrintToConsole($"Invalid convar: {entry.Key}");
      previousConvarValues[entry.Key] = getConvarStringValue(cv);
      setConvarStringValue(cv, entry.Value);
    }

    RoundUtil.SetTimeRemaining(Settings.RoundTime());

    if (Settings.StartInvulnerable) DisableDamage();

    if (Settings.StripToKnife || Settings.RespawnPlayers)
      foreach (var player in Utilities.GetPlayers()) {
        if (Settings.StripToKnife) {
          player.RemoveWeapons();
          player.GiveNamedItem("weapon_knife");
        }

        if (!Settings.RespawnPlayers) continue;
        if (player is {
          PawnIsAlive: false, Team : CsTeam.Terrorist or CsTeam.CounterTerrorist
        })
          player.Respawn();
      }

    if (!Settings.AllowLastRequests)
      provider.GetRequiredService<ILastRequestManager>().DisableLRForRound();
    if (!Settings.AllowLastGuard)
      provider.GetRequiredService<ILastGuardService>()
       .DisableLastGuardForRound();

    doTeleports();

    if (Settings.FreezePlayers)
      foreach (var player in Utilities.GetPlayers()) {
        player.Freeze();
        Plugin.AddTimer(Settings.FreezeTime(player),
          () => { player.UnFreeze(); });
      }

    if (Timers.Count > 0) {
      foreach (var entry in Timers) {
        Plugin.AddTimer(entry.Key, () => {
          if (provider.GetRequiredService<ISpecialDayManager>().CurrentSD
            != this)
            return;
          entry.Value.Invoke();
        }, TimerFlags.STOP_ON_MAPCHANGE);
      }
    }
  }

  private void doTeleports() {
    if (Settings == null) return;
    IEnumerable<CCSPlayerController> targets = [];

    if (Settings.ForceTeleportAll)
      targets = PlayerUtil.GetAlive();
    else
      targets = Settings.Teleport switch {
        SpecialDaySettings.TeleportType.CELL
          or SpecialDaySettings.TeleportType.CELL_STACKED =>
          PlayerUtil.FromTeam(CsTeam.CounterTerrorist),
        SpecialDaySettings.TeleportType.ARMORY
          or SpecialDaySettings.TeleportType.ARMORY_STACKED => PlayerUtil
           .FromTeam(CsTeam.Terrorist),
        SpecialDaySettings.TeleportType.RANDOM => PlayerUtil.GetAlive(),
        _                                      => targets
      };

    IEnumerable<Vector> spawnPositions;

    var tSpawns = Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!);
    var ctSpawns = Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!);
    var tpSpawns = Utilities
     .FindAllEntitiesByDesignerName<
        CInfoTeleportDestination>("info_teleport_destination")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!);
    switch (Settings.Teleport) {
      case SpecialDaySettings.TeleportType.CELL:
        spawnPositions = tSpawns;
        break;
      case SpecialDaySettings.TeleportType.CELL_STACKED:
        spawnPositions = [tSpawns.First()];
        break;
      case SpecialDaySettings.TeleportType.ARMORY:
        spawnPositions = ctSpawns;
        break;
      case SpecialDaySettings.TeleportType.ARMORY_STACKED:
        spawnPositions = [ctSpawns.First()];
        break;
      case SpecialDaySettings.TeleportType.RANDOM:
        // TODO: Support truly random spawns
        spawnPositions = tSpawns.Union(ctSpawns).Union(tpSpawns);
        break;
      case SpecialDaySettings.TeleportType.NONE:
      default:
        return;
    }

    var enumerable   = spawnPositions.ToList();
    var baggedSpawns = new ShuffleBag<Vector>(enumerable);

    foreach (var target in targets)
      target.Pawn.Value?.Teleport(baggedSpawns.GetNext());
  }

  private string getConvarStringValue(ConVar? cvar) {
    try {
      if (cvar == null) return "";
      var convarValue = cvar.Type switch {
        ConVarType.Bool => cvar.GetPrimitiveValue<bool>().ToString(),
        ConVarType.Float32 or ConVarType.Float64 => cvar
         .GetPrimitiveValue<float>()
         .ToString(),
        ConVarType.UInt16 => cvar.GetPrimitiveValue<ushort>().ToString(),
        ConVarType.Int16  => cvar.GetPrimitiveValue<short>().ToString(),
        ConVarType.UInt32 => cvar.GetPrimitiveValue<uint>().ToString(),
        ConVarType.Int32  => cvar.GetPrimitiveValue<int>().ToString(),
        ConVarType.Int64  => cvar.GetPrimitiveValue<long>().ToString(),
        ConVarType.UInt64 => cvar.GetPrimitiveValue<ulong>().ToString(),
        ConVarType.String => cvar.StringValue,
        _                 => ""
      };
      return convarValue;
    } catch (Exception) { return "INVALID"; }
  }

  private void setConvarStringValue(ConVar? cvar, object value) {
    if (cvar == null) return;
    try {
      switch (cvar.Type) {
        case ConVarType.Bool:
          cvar.SetValue((bool)value);
          break;
        case ConVarType.Float32 or ConVarType.Float64:
          cvar.SetValue((float)value);
          break;
        case ConVarType.UInt16:
          cvar.SetValue((ushort)value);
          break;
        case ConVarType.Int16:
          cvar.SetValue((short)value);
          break;
        case ConVarType.UInt32:
          cvar.SetValue((uint)value);
          break;
        case ConVarType.Int32:
          cvar.SetValue((int)value);
          break;
        case ConVarType.Int64:
          cvar.SetValue((long)value);
          break;
        case ConVarType.UInt64:
          cvar.SetValue((ulong)value);
          break;
        case ConVarType.String:
          cvar.SetValue((string)value);
          break;
      }
    } catch (InvalidOperationException) { }
  }

  /// <summary>
  ///   Called when the actual action begins for the special day.
  /// </summary>
  public virtual void Execute() { EnableDamage(); }

  [GameEventHandler]
  public virtual HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var entry in previousConvarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null || entry.Value == null) continue;
      try { setConvarStringValue(cv, entry.Value); } catch (
        InvalidOperationException e) { Console.WriteLine(e); }

      previousConvarValues.Clear();
    }

    Plugin.DeregisterEventHandler<EventRoundEnd>(OnEnd);
    return HookResult.Continue;
  }

  protected void DisableDamage() {
    foreach (var player in PlayerUtil.GetAlive()) DisableDamage(player);
  }

  protected void EnableDamage() {
    foreach (var player in PlayerUtil.GetAlive()) EnableDamage(player);
  }

  protected void DisableDamage(CCSPlayerController player) {
    if (player.Pawn.Value != null) player.Pawn.Value.TakesDamage = false;
  }

  protected void EnableDamage(CCSPlayerController player) {
    if (player.Pawn.Value != null) player.Pawn.Value.TakesDamage = true;
  }
}