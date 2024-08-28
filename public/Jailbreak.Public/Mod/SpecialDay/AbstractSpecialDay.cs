using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastGuard;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Mod.SpecialDay;

/// <summary>
///   Represents a special day that will last for the entire round and affect
///   all players.
/// </summary>
/// <param name="plugin"></param>
/// <param name="provider"></param>
public abstract class AbstractSpecialDay(BasePlugin plugin,
  IServiceProvider provider) {
  protected readonly BasePlugin Plugin = plugin;

  /// <summary>
  ///   Responsible for tracking what convars we've changed to be able
  ///   to change them back to their original values once the day ends.
  /// </summary>
  private readonly Dictionary<string, object?> previousConvarValues = new();

  protected readonly IServiceProvider Provider = provider;

  protected readonly IDictionary<float, Action> Timers =
    new DefaultableDictionary<float, Action>(new Dictionary<float, Action>(),
      () => { });

  public abstract SDType Type { get; }
  public virtual SpecialDaySettings Settings => new();

  /// <summary>
  ///   Called when the warden initially picks the special day.
  ///   Use for teleporting, stripping weapons, starting timers, etc.
  ///   Nearly every day should call base.Setup(). If you are overriding
  ///   this, make sure to look at what base.Setup() does and ensure you
  ///   know why you are not calling it.
  /// </summary>
  public virtual void Setup() {
    Plugin.RegisterFakeConVars(this);
    Plugin.RegisterEventHandler<EventRoundEnd>(OnEnd);
    Plugin.RegisterEventHandler<EventItemPickup>(OnPickup);

    foreach (var entry in Settings.ConVarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null) {
        Server.PrintToConsole($"Invalid convar: {entry.Key}");
        continue;
      }

      previousConvarValues[entry.Key] = GetConvarValue(cv);
      SetConvarValue(cv, entry.Value);
    }

    RoundUtil.SetTimeRemaining(Settings.RoundTime());

    foreach (var player in Utilities.GetPlayers()
     .Where(p => p.Team is CsTeam.Terrorist or CsTeam.CounterTerrorist)) {
      if (Settings.RespawnPlayers && !player.PawnIsAlive) player.Respawn();

      var val = Settings.InitialHealth(player);
      if (val != -1) player.SetHealth(val);
      val = Settings.InitialMaxHealth(player);
      if (val != -1) player.SetMaxHealth(val);
      val = Settings.InitialArmor(player);
      if (val != -1) player.SetArmor(val);

      if (Settings.StripToKnife) {
        player.RemoveWeapons();
        player.GiveNamedItem("weapon_knife");
      }
    }

    if (Settings.StartInvulnerable) DisableDamage();

    if (!Settings.AllowLastRequests)
      Provider.GetService<ILastRequestManager>()?.DisableLRForRound();
    if (!Settings.AllowLastGuard)
      Provider.GetService<ILastGuardService>()?.DisableLastGuardForRound();
    if (!Settings.AllowRebels)
      Provider.GetService<IRebelService>()?.DisableRebelForRound();

    if (Settings.OpenCells) {
      var zones = Provider.GetService<IZoneManager>();
      if (zones == null)
        MapUtil.OpenCells();
      else
        MapUtil.OpenCells(zones);
      var openCmd = Provider.GetService<IWardenOpenCommand>();
      if (openCmd != null) openCmd.OpenedCells = true;
    }

    doTeleports();

    if (Settings.FreezePlayers)
      foreach (var player in Utilities.GetPlayers()) {
        player.Freeze();
        Timers[Settings.FreezeTime(player)] += () => player.UnFreeze();
      }

    foreach (var entry in Timers)
      Plugin.AddTimer(entry.Key, () => {
        if (Provider.GetRequiredService<ISpecialDayManager>().CurrentSD != this)
          return;
        entry.Value.Invoke();
      }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void doTeleports() {
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
    var tVectors = tSpawns.ToArray();
    tVectors.Shuffle();
    var ctVectors = ctSpawns.ToArray();
    ctVectors.Shuffle();

    var                 mgr = Provider.GetService<IZoneManager>();
    IEnumerable<Vector> spawnPositions;
    switch (type) {
      case SpecialDaySettings.TeleportType.CELL:
        spawnPositions = tVectors;
        break;
      case SpecialDaySettings.TeleportType.CELL_STACKED:
        spawnPositions = [tVectors.First()];
        break;
      case SpecialDaySettings.TeleportType.ARMORY:
        spawnPositions = ctVectors;
        break;
      case SpecialDaySettings.TeleportType.ARMORY_STACKED:
        spawnPositions = [ctVectors.First()];
        break;
      case SpecialDaySettings.TeleportType.RANDOM:
        spawnPositions =
          MapUtil.GetRandomSpawns(PlayerUtil.GetAlive().Count(), mgr);
        break;
      case SpecialDaySettings.TeleportType.RANDOM_STACKED:
        spawnPositions = [MapUtil.GetRandomSpawns(1, mgr).First()];
        break;
      default:
        return;
    }

    var baggedSpawns = new ShuffleBag<Vector>(spawnPositions.ToList());
    foreach (var player in players)
      player.PlayerPawn.Value?.Teleport(baggedSpawns.GetNext());
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
    var previous = GetConvarValue(cvar);
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

      if (cvar.Name is "mp_teammates_are_enemies" or "sv_autobunnyhopping"
        or "mp_death_drop_gun") {
        // These convars require a frame to take effect, otherwise client-side
        // stuff is not properly updated
        Server.ExecuteCommand($"{cvar.Name} {previous}");
        Server.NextFrame(() => Server.ExecuteCommand($"{cvar.Name} {value}"));
      } else { Server.ExecuteCommand(cvar.Name + " " + value); }
    } catch (Exception e) {
      Server.PrintToChatAll(
        $"There was an error setting {cvar.Name} ({cvar.Type}) to {value} ({value.GetType()}");
      Server.PrintToConsole(e.Message);
      Server.PrintToConsole(e.StackTrace ?? "");
    }
  }

  /// <summary>
  ///   Called when the actual action begins for the special day.
  /// </summary>
  public virtual void Execute() { EnableDamage(); }

  virtual protected HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var entry in previousConvarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null || entry.Value == null) continue;
      SetConvarValue(cv, entry.Value);
    }

    previousConvarValues.Clear();

    Plugin.DeregisterEventHandler<EventRoundEnd>(OnEnd);
    Plugin.DeregisterEventHandler<EventItemPickup>(OnPickup);
    return HookResult.Continue;
  }

  virtual protected HookResult OnPickup(EventItemPickup @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    var allowed = Settings.AllowedWeapons(player);
    var weapon  = "weapon_" + @event.Item;
    if (allowed == null || allowed.Contains(@event.Item))
      return HookResult.Continue;
    player.RemoveItemByDesignerName(weapon, true);
    return HookResult.Continue;
  }

  [Obsolete("No longer used, you must manually register/unregister this")]
  virtual protected void OnTick() { }

  protected void DisableDamage() {
    foreach (var player in PlayerUtil.GetAlive()) DisableDamage(player);
  }

  protected void EnableDamage() {
    foreach (var player in PlayerUtil.GetAlive()) EnableDamage(player);
  }

  protected void DisableDamage(CCSPlayerController player) {
    if (!player.IsValid || player.Pawn.Value == null) return;
    if (!player.Pawn.IsValid) return;
    player.Pawn.Value.TakesDamage = false;
  }

  protected void EnableDamage(CCSPlayerController player) {
    if (!player.IsValid || player.Pawn.Value == null) return;
    if (!player.Pawn.IsValid) return;
    player.Pawn.Value.TakesDamage = true;
  }
}