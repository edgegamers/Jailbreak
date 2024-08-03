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
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Mod.SpecialDay;

public abstract class AbstractSpecialDay(BasePlugin plugin,
  IServiceProvider provider) {
  protected readonly BasePlugin Plugin = plugin;
  private readonly Dictionary<string, object?> previousConvarValues = new();
  protected readonly IServiceProvider Provider = provider;

  protected IDictionary<float, Action> Timers =
    new DefaultableDictionary<float, Action>(new Dictionary<float, Action>(),
      () => { });

  public abstract SDType Type { get; }
  public virtual SpecialDaySettings Settings => new();

  /// <summary>
  ///   Called when the warden initially picks the special day.
  ///   Use for teleporting, stripping weapons, starting timers, etc.
  /// </summary>
  public virtual void Setup() {
    Plugin.RegisterAllAttributes(this);
    Plugin.RegisterEventHandler<EventRoundEnd>(OnEnd);

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
      Provider.GetRequiredService<ILastRequestManager>().DisableLRForRound();
    if (!Settings.AllowLastGuard)
      Provider.GetRequiredService<ILastGuardService>()
       .DisableLastGuardForRound();
    if (!Settings.AllowRebels)
      Provider.GetRequiredService<IRebelService>().DisableRebelForRound();

    if (Settings.OpenCells)
      MapUtil.OpenCells(Provider.GetRequiredService<IZoneManager>());

    doTeleports();

    if (Settings.FreezePlayers)
      foreach (var player in Utilities.GetPlayers()) {
        player.Freeze();
        Plugin.AddTimer(Settings.FreezeTime(player),
          () => { player.UnFreeze(); });
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
    var enumerable = tSpawns as Vector[] ?? tSpawns.ToArray();
    enumerable.Shuffle();
    var positions = ctSpawns as Vector[] ?? ctSpawns.ToArray();
    positions.Shuffle();

    IEnumerable<Vector> spawnPositions;
    switch (type) {
      case SpecialDaySettings.TeleportType.CELL:
        spawnPositions = enumerable;
        break;
      case SpecialDaySettings.TeleportType.CELL_STACKED:
        spawnPositions = [enumerable.First()];
        break;
      case SpecialDaySettings.TeleportType.ARMORY:
        spawnPositions = positions;
        break;
      case SpecialDaySettings.TeleportType.ARMORY_STACKED:
        spawnPositions = [positions.First()];
        break;
      case SpecialDaySettings.TeleportType.RANDOM:
        spawnPositions = getAtLeastRandom(PlayerUtil.GetAlive().Count());
        break;
      case SpecialDaySettings.TeleportType.RANDOM_STACKED:
        spawnPositions = getAtLeastRandom(1);
        break;
      default:
        return;
    }

    var baggedSpawns = new ShuffleBag<Vector>(spawnPositions.ToList());
    foreach (var player in players)
      player.PlayerPawn.Value?.Teleport(baggedSpawns.GetNext());
  }

  private List<Vector> getAtLeastRandom(int count) {
    // Progressively get more lax with our "randomness quality"
    var result                       = getRandomSpawns(false, false, false);
    if (result.Count < count) result = getRandomSpawns(false, false);
    if (result.Count < count) result = getRandomSpawns(false);
    if (result.Count < count) result = getRandomSpawns();
    return result;
  }

  private List<Vector> getRandomSpawns(bool includeSpawns = true,
    bool includeTps = true, bool includeAuto = true) {
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

    var zoneManager = Provider.GetRequiredService<IZoneManager>();
    if (includeAuto)
      result.AddRange(zoneManager.GetZones(ZoneType.SPAWN_AUTO)
       .GetAwaiter()
       .GetResult()
       .Select(z => z.GetCenterPoint()));

    var zones = zoneManager.GetZones(ZoneType.SPAWN).GetAwaiter().GetResult();
    result.AddRange(zones.Select(z => z.GetCenterPoint()));

    result.Shuffle();

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
      if (cvar.Name == "mp_teammates_are_enemies") {
        var opposite = !(bool)value;
        Server.ExecuteCommand("css_cvar mp_teammates_are_enemies " + opposite);

        Server.NextFrame(() => {
          Server.ExecuteCommand("css_cvar mp_teammates_are_enemies " + value);
        });
      }
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
  public virtual void Execute() {
    EnableDamage();
    if (Settings.RestrictWeapons)
      Plugin.RegisterListener<Listeners.OnTick>(OnTick);
  }

  virtual protected void OnTick() {
    foreach (var player in PlayerUtil.GetAlive()) {
      var weapons = Settings.AllowedWeapons(player);
      if (weapons == null) continue;
      disableWeapon(player, weapons);
    }
  }

  private void disableWeapon(CCSPlayerController player,
    ICollection<string> allowed) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    if (allowed.Contains(activeWeapon.DesignerName)) return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
    activeWeapon.NextPrimaryAttackTick   = Server.TickCount + 500;
  }

  virtual protected HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var entry in previousConvarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null || entry.Value == null) continue;
      SetConvarValue(cv, entry.Value);
    }

    previousConvarValues.Clear();

    if (Settings.RestrictWeapons)
      Plugin.RemoveListener<Listeners.OnTick>(OnTick);

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