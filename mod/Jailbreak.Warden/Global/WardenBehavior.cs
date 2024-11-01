using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl.Stats;
using GangsAPI.Data;
using GangsAPI.Services;
using GangsAPI.Services.Player;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Jailbreak.Zones;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MStatsShared;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Warden.Global;

// By making it a struct we ensure values from the CCSPlayerPawn are passed by VALUE.
public struct PreWardenStats(int armorValue, int health, int maxHealth,
  bool headHealthShot, bool hadHelmetArmor) {
  public readonly int ArmorValue = armorValue;
  public readonly int Health = health;
  public readonly int MaxHealth = maxHealth;
  public readonly bool HeadHealthShot = headHealthShot;
  public readonly bool HadHelmetArmor = hadHelmetArmor;
}

public class WardenBehavior(ILogger<WardenBehavior> logger,
  IWardenLocale locale, IRichLogService logs,
  ISpecialTreatmentService specialTreatment, IRebelService rebels,
  IMuteService mute, IServiceProvider provider)
  : IPluginBehavior, IWardenService {
  public static readonly FakeConVar<int> CV_ARMOR_EQUAL = new("css_jb_hp_equal",
    "Health points for when CTs have equal ratio", 50, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_ARMOR_OUTNUMBER =
    new("css_jb_hp_outnumber", "HP for CTs when outnumbering Ts", 25,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_ARMOR_OUTNUMBERED =
    new("css_jb_hp_outnumbered", "Health points for CTs when outnumbered by Ts",
      100, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_WARDEN_ARMOR =
    new("css_jb_warden_armor", "Armor for the warden", 125,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_WARDEN_AUTO_OPEN_CELLS =
    new("css_jb_warden_opencells_delay",
      "Delay in seconds to auto-open cells at, -1 to disable", 60);

  public static readonly FakeConVar<bool> CV_WARDEN_AUTO_SNITCH =
    new("css_jb_warden_auto_snitch",
      "True to broadcast how many prisoners were in cells when they auto-open",
      true);

  public static readonly FakeConVar<int> CV_WARDEN_HEALTH =
    new("css_jb_warden_hp", "HP for the warden", 125, ConVarFlags.FCVAR_NONE,
      new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_WARDEN_MAX_HEALTH =
    new("css_jb_warden_maxhp", "Max HP for the warden", 100,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<string> CV_WARDEN_SOUND_KILLED =
    new("css_jb_warden_sound_killed", "Sound to play when the warden is killed",
      "wardenKilled");

  public static readonly FakeConVar<string> CV_WARDEN_SOUND_PASSED =
    new("css_jb_warden_sound_killed", "Sound to play when the warden passes",
      "wardenPassed");

  public static readonly FakeConVar<string> CV_WARDEN_SOUND_NEW =
    new("css_jb_warden_sound_killed",
      "Sound to play when the warden is assigned", "wardenNew");

  public static readonly FakeConVar<int> CV_WARDEN_TERRORIST_RATIO =
    new("css_jb_warden_t_ratio", "Ratio of T:CT to use for HP adjustments", 3);

  private readonly ISet<CCSPlayerController> bluePrisoners =
    new HashSet<CCSPlayerController>();

  private bool firstWarden;
  private string? oldTag;
  private char? oldTagColor;

  private BasePlugin parent = null!;
  private PreWardenStats? preWardenStats;
  private Timer? unblueTimer, openCellsTimer;

  public void Start(BasePlugin basePlugin) {
    parent = basePlugin;
    if (API.Gangs != null) {
      var stats = API.Gangs.Services.GetService<IStatManager>();
      if (stats == null) return;
      stats.Stats.Add(new WardenStat());
    }
  }

  /// <summary>
  ///   Get the current warden, if there is one.
  /// </summary>
  public CCSPlayerController? Warden { get; private set; }

  /// <summary>
  ///   Whether or not a warden is currently assigned
  /// </summary>
  public bool HasWarden { get; private set; }

  public bool TrySetWarden(CCSPlayerController controller) {
    if (HasWarden) return false;

    //	Verify player is a CT
    if (controller.Team != CsTeam.CounterTerrorist) return false;
    if (!controller.PawnIsAlive) return false;

    mute.UnPeaceMute();

    HasWarden = true;
    Warden    = controller;
    Warden.SetColor(Color.Blue);

    locale.NewWarden(Warden).ToAllChat().ToAllCenter();

    Warden.Clan = "[WARDEN]";
    Utilities.SetStateChanged(Warden, "CCSPlayerController", "m_szClan");
    var ev = new EventNextlevelChanged(true);
    ev.FireEvent(false);

    API.Stats?.PushStat(new ServerStat("JB_WARDEN_ASSIGNED",
      Warden.SteamID.ToString()));

    if (API.Actain != null) {
      var steam = Warden.SteamID;
      Server.NextFrameAsync(async () => {
        oldTag      = await API.Actain.getTagService().GetTag(steam);
        oldTagColor = await API.Actain.getTagService().GetTagColor(steam);
        Server.NextFrame(() => {
          if (!Warden.IsValid) return;
          API.Actain.getTagService().SetTag(Warden, "[WARDEN]", false);
          API.Actain.getTagService()
           .SetTagColor(Warden, ChatColors.DarkBlue, false);
        });
      });
    }

    if (API.Gangs != null) {
      var wrapper = new PlayerWrapper(Warden);
      var stats   = API.Gangs.Services.GetService<IPlayerStatManager>();
      if (stats != null) {
        Task.Run(async () => {
          var (success, stat) =
            await stats.GetForPlayer<WardenData>(wrapper, WardenStat.STAT_ID);

          if (!success || stat == null) stat = new WardenData();

          stat.TimesWardened++;
          await stats.SetForPlayer(wrapper, WardenStat.STAT_ID, stat);
        });
      }
    }

    foreach (var player in Utilities.GetPlayers())
      player.ExecuteClientCommand($"play sounds/{CV_WARDEN_SOUND_NEW.Value}");

    logs.Append(logs.Player(Warden), "is now the warden.");

    unblueTimer = parent.AddTimer(3, unmarkPrisonersBlue);
    mute.PeaceMute(firstWarden ?
      MuteReason.INITIAL_WARDEN :
      MuteReason.WARDEN_TAKEN);

    // Always store the stats of the warden b4 they became warden, in case we need to restore them later.
    var wardenPawn = Warden.PlayerPawn.Value;
    if (wardenPawn == null) return false;

    if (firstWarden) {
      firstWarden = false;

      var hasHealthshot = playerHasHealthshot(Warden);
      var hasHelmet     = playerHasHelmetArmor(Warden);
      preWardenStats = new PreWardenStats(wardenPawn.ArmorValue,
        wardenPawn.Health, wardenPawn.MaxHealth, hasHealthshot, hasHelmet);

      if (!hasHelmet) Warden.GiveNamedItem("item_assaultsuit");

      var ctArmorValue = getBalance() switch {
        0  => CV_ARMOR_EQUAL.Value,       // Balanced teams
        1  => CV_ARMOR_OUTNUMBERED.Value, // Ts outnumber CTs
        -1 => CV_ARMOR_OUTNUMBER.Value,   // CTs outnumber Ts
        _  => CV_ARMOR_EQUAL.Value        // default (should never happen)
      };

      /* Round start CT buff */
      foreach (var guardController in Utilities.GetPlayers()
       .Where(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true })) {
        var guardPawn = guardController.PlayerPawn.Value;
        if (guardPawn == null) continue;

        guardPawn.ArmorValue = ctArmorValue;
        Utilities.SetStateChanged(guardPawn, "CCSPlayerPawn", "m_ArmorValue");
      }

      setWardenStats(wardenPawn, CV_WARDEN_ARMOR.Value, CV_WARDEN_HEALTH.Value,
        CV_WARDEN_MAX_HEALTH.Value);
      if (!hasHealthshot) Warden.GiveNamedItem("weapon_healthshot");
    } else { preWardenStats = null; }

    return true;
  }

  public bool TryRemoveWarden(bool isPass = false) {
    if (!HasWarden) return false;

    mute.UnPeaceMute();

    HasWarden = false;

    if (Warden != null && Warden.Pawn.Value != null) {
      Warden.Clan = "";
      Warden.SetColor(Color.White);
      Utilities.SetStateChanged(Warden, "CCSPlayerController", "m_szClan");
      var ev = new EventNextlevelChanged(true);
      ev.FireEvent(false);

      if (oldTag != null)
        API.Actain?.getTagService().SetTag(Warden, oldTag, false);
      if (oldTagColor != null)
        API.Actain?.getTagService()
         .SetTagColor(Warden, oldTagColor.Value, false);

      logs.Append(logs.Player(Warden), "is no longer the warden.");

      if (!isPass) {
        var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
        if (stats != null) {
          var wrapper = new PlayerWrapper(Warden);
          Task.Run(async () => await updateWardenDeathStats(wrapper));
        }
      }
    }

    var wardenPawn = Warden!.PlayerPawn.Value;
    if (wardenPawn == null) return false;

    // if isPass we restore their old health values or their current health, whichever is less.
    if (isPass && preWardenStats != null) {
      // Regardless of if the above if statement is true or false, we want to restore the player's previous stats.
      setWardenStats(wardenPawn,
        Math.Min(wardenPawn.ArmorValue, preWardenStats.Value.ArmorValue),
        Math.Min(wardenPawn.Health, preWardenStats.Value.Health),
        Math.Min(wardenPawn.MaxHealth, preWardenStats.Value.MaxHealth));

      /* This code makes sure people can't abuse the first warden's buff by removing it if they pass. */
      var itemServices = itemServicesOrNull(Warden);
      if (itemServices == null) return false;

      if (!preWardenStats.Value.HadHelmetArmor) itemServices.HasHelmet = false;

      Utilities.SetStateChanged(wardenPawn, "CBasePlayerPawn",
        "m_pItemServices");

      if (!preWardenStats.Value.HeadHealthShot)
        playerHasHealthshot(Warden, true);
    }

    Warden = null;
    return true;
  }

  private async Task updateWardenDeathStats(PlayerWrapper player) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return;

    var (success, stat) =
      await stats.GetForPlayer<WardenData>(player, WardenStat.STAT_ID);
    if (!success || stat == null) stat = new WardenData();
    stat.WardenDeaths++;

    await stats.SetForPlayer(player, WardenStat.STAT_ID, stat);
  }

  [GameEventHandler]
  public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) {
    var player = ev.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    var isWarden = ((IWardenService)this).IsWarden(ev.Userid);
    if (API.Gangs != null) {
      if (ev.Attacker != null && ev.Attacker.IsValid && ev.Attacker != player
        && isWarden) {
        var wrapper = new PlayerWrapper(ev.Attacker);
        Task.Run(async () => await incrementWardenKills(wrapper));
      }

      foreach (var guard in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        var wrapper = new PlayerWrapper(guard);
        // If the guard is the warden, update all guards' stats
        // If the guard is not the warden, only update the warden's stats
        if (guard.SteamID == player.SteamID != isWarden) continue;
        Task.Run(async () => await updateGuardDeathStats(wrapper, isWarden));
      }
    }

    if (!isWarden) return HookResult.Continue;

    API.Stats?.PushStat(new ServerStat("JB_WARDEN_DEATH"));

    mute.UnPeaceMute();
    processWardenDeath();
    return HookResult.Continue;
  }

  private async Task incrementWardenKills(PlayerWrapper attacker) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return;
    await Task.Run(async () => {
      var (success, stat) =
        await stats.GetForPlayer<WardenData>(attacker, WardenStat.STAT_ID);

      if (!success || stat == null) stat = new WardenData();

      stat.WardensKilled++;
      await stats.SetForPlayer(attacker, WardenStat.STAT_ID, stat);
    });
  }

  private async Task
    updateGuardDeathStats(PlayerWrapper player, bool isWarden) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return;

    var (success, stat) =
      await stats.GetForPlayer<WardenData>(player, WardenStat.STAT_ID);

    if (!success || stat == null) stat = new WardenData();

    if (isWarden) {
      // The warden let a guard die
      stat.GuardDeathsAsWarden++;
    } else {
      // The guard let the warden die
      stat.WardenDeathsAsGuard++;
    }

    await stats.SetForPlayer(player, WardenStat.STAT_ID, stat);
  }

  [GameEventHandler]
  public HookResult OnChangeTeam(EventPlayerTeam @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;

    if (API.Actain != null) {
      var steam = player.SteamID;
      Server.NextFrameAsync(async () => {
        if ("[WARDEN]" != await API.Actain.getTagService().GetTag(steam))
          return;
        Server.NextFrame(() => {
          if (!player.IsValid) return;
          API.Actain.getTagService().SetTag(player, "", false);
          API.Actain.getTagService()
           .SetTagColor(player, ChatColors.Default, false);
        });
      });
    }

    if (!((IWardenService)this).IsWarden(player)) return HookResult.Continue;

    mute.UnPeaceMute();
    processWardenDeath();
    return HookResult.Continue;
  }

  private void processWardenDeath() {
    if (!TryRemoveWarden())
      logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

    //	Warden died!
    locale.WardenDied.ToAllChat().ToAllCenter();

    foreach (var player in Utilities.GetPlayers()) {
      if (!player.IsReal()) continue;
      player.ExecuteClientCommand(
        $"play sounds/{CV_WARDEN_SOUND_KILLED.Value}");
    }

    locale.BecomeNextWarden.ToAllChat();

    unblueTimer
    ?.Kill(); // If the warden dies withing 3 seconds of becoming warden, we need to cancel the unblue timer
    markPrisonersBlue();
  }

  private void unmarkPrisonersBlue() {
    foreach (var player in bluePrisoners) {
      if (!player.IsReal()) continue;
      if (ignoreColor(player)) continue;
      player.SetColor(Color.White);
    }

    bluePrisoners.Clear();
  }

  private void markPrisonersBlue() {
    foreach (var player in Utilities.GetPlayers()) {
      if (!player.IsReal() || player.Team != CsTeam.Terrorist) continue;
      if (ignoreColor(player)) continue;

      player.SetColor(Color.Blue);

      bluePrisoners.Add(player);
    }
  }

  private bool ignoreColor(CCSPlayerController player) {
    if (specialTreatment.IsSpecialTreatment(player)) return true;
    if (rebels.IsRebel(player)) return true;
    return false;
  }

  private int getBalance() {
    var ctCount = Utilities.GetPlayers()
     .Count(p => p.Team == CsTeam.CounterTerrorist);
    var tCount = Utilities.GetPlayers().Count(p => p.Team == CsTeam.Terrorist);

    // Casting to a float ensures if we're diving by zero, we get infinity instead of an error.
    var ratio = (float)tCount / CV_WARDEN_TERRORIST_RATIO.Value - ctCount;

    return ratio switch {
      > 0 => 1,
      0   => 0,
      _   => -1
    };
  }

  private CCSPlayer_ItemServices?
    itemServicesOrNull(CCSPlayerController player) {
    var itemServices = player.PlayerPawn.Value?.ItemServices;
    return itemServices != null ?
      new CCSPlayer_ItemServices(itemServices.Handle) :
      null;
  }

  private void setWardenStats(CCSPlayerPawn wardenPawn, int armor = -1,
    int health = -1, int maxHealth = -1) {
    if (armor != -1) {
      wardenPawn.ArmorValue = armor;
      Utilities.SetStateChanged(wardenPawn, "CCSPlayerPawn", "m_ArmorValue");
    }

    if (health != -1) {
      wardenPawn.Health = health;
      Utilities.SetStateChanged(wardenPawn, "CBaseEntity", "m_iHealth");
    }

    if (maxHealth != -1) {
      wardenPawn.MaxHealth = maxHealth;
      Utilities.SetStateChanged(wardenPawn, "CBaseEntity", "m_iMaxHealth");
    }
  }

  private bool playerHasHelmetArmor(CCSPlayerController player) {
    var itemServices = itemServicesOrNull(player);
    return itemServices is { HasHelmet: true };
  }

  private bool playerHasHealthshot(CCSPlayerController player,
    bool removeIfHas = false) {
    var playerPawn = player.PlayerPawn.Value;
    if (playerPawn == null || playerPawn.WeaponServices == null) return false;

    foreach (var weapon in playerPawn.WeaponServices.MyWeapons) {
      if (weapon.Value == null) continue;
      if (weapon.Value.DesignerName.Equals("weapon_healthshot")) {
        if (removeIfHas) weapon.Value.Remove();

        return true;
      }
    }

    return false;
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info) {
    TryRemoveWarden();
    mute.UnPeaceMute();
    openCellsTimer?.Kill();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    firstWarden    = true;
    preWardenStats = null;

    if (CV_WARDEN_AUTO_OPEN_CELLS.Value < 0 || RoundUtil.IsWarmup())
      return HookResult.Continue;
    var openCmd = provider.GetService<IWardenOpenCommand>();
    if (openCmd == null) return HookResult.Continue;
    var cmdLocale = provider.GetRequiredService<IWardenCmdOpenLocale>();

    openCellsTimer?.Kill();
    openCellsTimer = parent.AddTimer(CV_WARDEN_AUTO_OPEN_CELLS.Value, () => {
      var cellZone = getCellZone();

      var prisoners = PlayerUtil.FromTeam(CsTeam.Terrorist)
       .Count(p => p.Pawn.Value != null && p.PlayerPawn.Value?.AbsOrigin != null
          && cellZone.IsInsideZone(p.PlayerPawn.Value?.AbsOrigin!));

      if (openCmd.OpenedCells) {
        if (CV_WARDEN_AUTO_SNITCH.Value && prisoners > 0)
          cmdLocale.CellsOpenedSnitchPrisoners(prisoners);
        return;
      }

      var zoneMgr = provider.GetService<IZoneManager>();
      // Regardless of if we actually _detect_ prisoners in cells,
      // we should still open them (for convenience)
      if (zoneMgr != null)
        MapUtil.OpenCells(zoneMgr);
      else
        MapUtil.OpenCells();

      // Only if we detect prisoners in cells (i.e. presumably
      // cells haven't been opened yet) should we send the message

      if (prisoners == 0) return;

      if (CV_WARDEN_AUTO_SNITCH.Value)
        cmdLocale.CellsOpenedWithPrisoners(prisoners).ToAllChat();
      else
        cmdLocale.CellsOpened.ToAllChat();
    });

    return HookResult.Continue;
  }

  private IZone getCellZone() {
    var manager = provider.GetService<IZoneManager>();
    if (manager != null) {
      var zones = manager.GetZones(Server.MapName, ZoneType.CELL)
       .GetAwaiter()
       .GetResult();
      if (zones.Count > 0) return new MultiZoneWrapper(zones);
    }

    var bounds = new DistanceZone(
      Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!)
       .ToList(), DistanceZone.WIDTH_CELL);
    return bounds;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev,
    GameEventInfo info) {
    if (!((IWardenService)this).IsWarden(ev.Userid)) return HookResult.Continue;

    if (!TryRemoveWarden())
      logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


    locale.WardenLeft.ToAllChat().ToAllCenter();

    foreach (var player in Utilities.GetPlayers())
      player.ExecuteClientCommand(
        $"play sounds/{CV_WARDEN_SOUND_PASSED.Value}");

    locale.BecomeNextWarden.ToAllChat();
    return HookResult.Continue;
  }
}