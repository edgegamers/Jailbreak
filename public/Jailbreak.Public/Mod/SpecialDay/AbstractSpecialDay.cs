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
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Mod.SpecialDay;

public abstract class AbstractSpecialDay(BasePlugin plugin,
  IServiceProvider provider) {
  private readonly Dictionary<string, object?> previousConvarValues = new();
  protected BasePlugin Plugin = plugin;

  protected IDictionary<float, Action> Timers =
    new DefaultableDictionary<float, Action>(new Dictionary<float, Action>(),
      () => { });

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
      if (cv == null) {
        Server.PrintToConsole($"Invalid convar: {entry.Key}");
        continue;
      }

      previousConvarValues[entry.Key] = GetConvarValue(cv);
      Server.PrintToChatAll(
        $"Storing {entry.Key} ({cv.Type}) as {previousConvarValues[entry.Key]}");
      SetConvarValue(cv, entry.Value);
    }

    RoundUtil.SetTimeRemaining(Settings.RoundTime());

    foreach (var player in Utilities.GetPlayers()) {
      var val = Settings.InitialHealth(player);
      if (val != -1) player.SetHealth(Settings.InitialHealth(player));
      val = Settings.InitialMaxHealth(player);
      if (val != -1) player.SetMaxHealth(val);
      val = Settings.InitialArmor(player);
      if (val != -1) player.SetArmor(val);

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

    if (Settings.StartInvulnerable) DisableDamage();

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

    if (Timers.Count > 0)
      foreach (var entry in Timers)
        Plugin.AddTimer(entry.Key, () => {
          if (provider.GetRequiredService<ISpecialDayManager>().CurrentSD
            != this)
            return;
          entry.Value.Invoke();
        }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void doTeleports() {
    if (Settings == null) return;
    if (Settings.CtTeleport == Settings.TTeleport) {
      // If the teleports are the same, just do it once
      // this ensures the same bag is used for both teams
      doTeleports(Settings.CtTeleport, PlayerUtil.GetAlive());
      return;
    }

    doTeleports(Settings.CtTeleport,
      PlayerUtil.FromTeam(CsTeam.CounterTerrorist));
    doTeleports(Settings.TTeleport, PlayerUtil.FromTeam(CsTeam.Terrorist));
  }

  private void doTeleports(SpecialDaySettings.TeleportType type,
    IEnumerable<CCSPlayerController> players) {
    if (type == SpecialDaySettings.TeleportType.NONE) return;

    var tSpawns = Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!);
    var ctSpawns = Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!);
    IEnumerable<Vector> spawnPositions;
    switch (type) {
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
        spawnPositions = getRandomSpawns(false, false).ToList();
        // If we don't have enough manually specified spawns,
        // gradually pull from the other spawn types
        if (spawnPositions.Count() < PlayerUtil.GetAlive().Count()) {
          spawnPositions = getRandomSpawns(true, false).ToList();
          if (spawnPositions.Count() < PlayerUtil.GetAlive().Count())
            spawnPositions = getRandomSpawns().ToList();
        }

        break;
      default:
        return;
    }

    var baggedSpawns = new ShuffleBag<Vector>(spawnPositions.ToList());
    foreach (var player in players)
      player.Pawn.Value?.Teleport(baggedSpawns.GetNext());
  }

  private IEnumerable<Vector> getRandomSpawns(bool includeSpawns = true,
    bool includeTps = true) {
    var result = new List<Vector>();

    if (includeTps) {
      var worldTp = Utilities
       .FindAllEntitiesByDesignerName<CInfoTeleportDestination>(
          "info_teleport_destination")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!);
      result.AddRange(worldTp);
    }

    if (includeSpawns) {
      var tSpawns = Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!);
      var ctSpawns = Utilities
       .FindAllEntitiesByDesignerName<
          SpawnPoint>("info_player_counterterrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!);
      result.AddRange(tSpawns);
      result.AddRange(ctSpawns);
    }

    var zoneManager = provider.GetRequiredService<IZoneManager>();
    var zones = zoneManager.GetZones(ZoneType.SPAWN).GetAwaiter().GetResult();
    result.AddRange(zones.Select(z => z.GetCenterPoint()));

    return result;
  }

  protected object GetConvarValue(ConVar? cvar) {
    try {
      if (cvar == null) return "";
      object convarValue = cvar.Type switch {
        ConVarType.Bool => cvar.GetPrimitiveValue<bool>(),
        ConVarType.Float32 or ConVarType.Float64 => cvar
         .GetPrimitiveValue<float>(),
        ConVarType.UInt16 => cvar.GetPrimitiveValue<ushort>(),
        ConVarType.Int16  => cvar.GetPrimitiveValue<short>(),
        ConVarType.UInt32 => cvar.GetPrimitiveValue<uint>(),
        ConVarType.Int32  => cvar.GetPrimitiveValue<int>(),
        ConVarType.Int64  => cvar.GetPrimitiveValue<long>(),
        ConVarType.UInt64 => cvar.GetPrimitiveValue<ulong>(),
        _                 => cvar.StringValue
      };
      return convarValue;
    } catch (Exception e) {
      Server.PrintToChatAll(
        $"There was an error getting {cvar?.Name} ({cvar?.Type})");
      Server.PrintToConsole(e.Message);
      Server.PrintToConsole(e.StackTrace ?? "");
      return "";
    }
  }

  protected void SetConvarValue(ConVar? cvar, object value) {
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

      Server.ExecuteCommand(cvar.Name + " " + value);
      Server.PrintToChatAll($"{cvar.Name} {value}");
    } catch (Exception e) {
      Server.PrintToChatAll(
        $"There was an error setting {cvar.Name} ({cvar.Type}) to {value}");
      Server.PrintToConsole(e.Message);
      Server.PrintToConsole(e.StackTrace ?? "");
    }
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
      try { SetConvarValue(cv, entry.Value); } catch (InvalidOperationException
        e) { Console.WriteLine(e); }
    }

    previousConvarValues.Clear();

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