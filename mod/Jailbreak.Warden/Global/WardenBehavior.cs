using System.Drawing;
using api.plugin;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Warden.Global;

// By making it a struct we ensure values from the CCSPlayerPawn are passed by VALUE.
internal struct PreWardenStats(int armorValue, int health, int maxHealth,
  bool headHealthShot, bool hadHelmetArmor) {
  public readonly int ArmorValue = armorValue;
  public readonly int Health = health;
  public readonly int MaxHealth = maxHealth;
  public readonly bool HeadHealthShot = headHealthShot;
  public readonly bool HadHelmetArmor = hadHelmetArmor;
}

public class WardenBehavior(ILogger<WardenBehavior> logger,
  IWardenNotifications notifications, IRichLogService logs,
  ISpecialTreatmentService specialTreatment, IRebelService rebels,
  WardenConfig config, IMuteService mute) : IPluginBehavior, IWardenService {
  private readonly ISet<CCSPlayerController> bluePrisoners =
    new HashSet<CCSPlayerController>();

  private bool firstWarden;
  private string? oldTag;
  private char? oldTagColor;

  private BasePlugin? parent;
  private PreWardenStats? preWardenStats;
  private Timer? unblueTimer;

  private IActain? actain {
    get {
      try { return MAULCapability.Get(); } catch (KeyNotFoundException) {
        return null;
      }
    }
  }

  public static PluginCapability<IActain?> MAULCapability { get; } =
    new("maulactain:core");

  public readonly FakeConVar<int> CvArmorOutnumbered =
    new("css_jb_hp_outnumbered", "Health points for CTs when outnumbered by Ts",
      100, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public readonly FakeConVar<int> CvArmorEqual = new("css_jb_hp_outnumbered",
    "Health points for CTs have equal balance", 50, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public readonly FakeConVar<int> CvArmorOutnumber = new("css_jb_hp_outnumber",
    "HP for CTs when outnumbering Ts", 25, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public readonly FakeConVar<int> CvWardenHealth = new("css_jb_warden_hp",
    "HP for the warden", 125, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public readonly FakeConVar<int> CvWardenArmor = new("css_jb_warden_armor",
    "Armor for the warden", 125, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public readonly FakeConVar<int> CvWardenMaxHealth = new("css_jb_warden_maxhp",
    "Max HP for the warden", 100, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public void Start(BasePlugin basePlugin) { parent = basePlugin; }

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
    if (controller.GetTeam() != CsTeam.CounterTerrorist) return false;
    if (!controller.PawnIsAlive) return false;

    mute.UnPeaceMute();

    HasWarden = true;
    Warden    = controller;

    if (Warden.Pawn.Value != null) {
      Warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
      Warden.Pawn.Value.Render     = Color.FromArgb(254, 0, 0, 255);
      Utilities.SetStateChanged(Warden.Pawn.Value, "CBaseModelEntity",
        "m_clrRender");
    }

    notifications.NewWarden(Warden).ToAllChat().ToAllCenter();

    Warden.Clan = "[WARDEN]";
    Utilities.SetStateChanged(Warden, "CCSPlayerController", "m_szClan");
    var ev = new EventNextlevelChanged(true);
    ev.FireEvent(false);

    oldTag      = actain?.getTagService().GetTag(Warden);
    oldTagColor = actain?.getTagService().GetTagColor(Warden);
    Server.PrintToConsole("Do we have actain? " + (actain != null));
    actain?.getTagService().SetTag(Warden, "[WARDEN]");
    actain?.getTagService().SetTagColor(Warden, ChatColors.DarkBlue);

    foreach (var player in Utilities.GetPlayers())
      player.ExecuteClientCommand($"play sounds/{config.WardenNewSoundName}");

    logs.Append(logs.Player(Warden), "is now the warden.");

    unblueTimer = parent!.AddTimer(3, unmarkPrisonersBlue);
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

      setWardenStats(wardenPawn, CvWardenArmor.Value, CvWardenHealth.Value,
        CvWardenMaxHealth.Value);
      if (!hasHealthshot) Warden.GiveNamedItem("weapon_healthshot");
    } else { preWardenStats = null; }

    return true;
  }

  public bool TryRemoveWarden(bool isPass = false) {
    if (!HasWarden) return false;

    mute.UnPeaceMute();

    HasWarden = false;

    if (Warden != null && Warden.Pawn.Value != null) {
      Warden.Clan                  = "";
      Warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
      Warden.Pawn.Value.Render     = Color.FromArgb(254, 255, 255, 255);
      Utilities.SetStateChanged(Warden.Pawn.Value, "CBaseModelEntity",
        "m_clrRender");
      Utilities.SetStateChanged(Warden, "CCSPlayerController", "m_szClan");
      var ev = new EventNextlevelChanged(true);
      ev.FireEvent(false);

      actain?.getTagService().SetTag(Warden, oldTag!);
      if (oldTagColor != null)
        actain?.getTagService().SetTagColor(Warden, oldTagColor.Value);

      logs.Append(logs.Player(Warden), "is no longer the warden.");
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

  [GameEventHandler]
  public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) {
    if (!((IWardenService)this).IsWarden(ev.Userid)) return HookResult.Continue;

    mute.UnPeaceMute();
    processWardenDeath();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnChangeTeam(EventPlayerTeam @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;

    if ("[WARDEN]" == actain?.getTagService().GetTag(player)) {
      actain.getTagService().SetTag(player, "");
      actain.getTagService().SetTagColor(player, ChatColors.Default);
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
    notifications.WardenDied.ToAllChat().ToAllCenter();

    foreach (var player in Utilities.GetPlayers()) {
      if (!player.IsReal()) continue;
      player.ExecuteClientCommand(
        $"play sounds/{config.WardenKilledSoundName}");
    }

    notifications.BecomeNextWarden.ToAllChat();

    unblueTimer
    ?.Kill(); // If the warden dies withing 3 seconds of becoming warden, we need to cancel the unblue timer
    markPrisonersBlue();
  }

  private void unmarkPrisonersBlue() {
    foreach (var player in bluePrisoners) {
      if (!player.IsReal()) continue;
      var pawn = player.Pawn.Value;
      if (pawn == null) continue;
      if (ignoreColor(player)) continue;
      pawn.RenderMode = RenderMode_t.kRenderNormal;
      pawn.Render     = Color.FromArgb(254, 255, 255, 255);
      Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
    }

    bluePrisoners.Clear();
  }

  private void markPrisonersBlue() {
    foreach (var player in Utilities.GetPlayers()) {
      if (!player.IsReal() || player.Team != CsTeam.Terrorist) continue;
      if (ignoreColor(player)) continue;

      var pawn = player.Pawn.Value;
      if (pawn == null) continue;
      pawn.RenderMode = RenderMode_t.kRenderTransColor;
      pawn.Render     = Color.FromArgb(254, 0, 0, 255);
      Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

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
    var ratio = (float)tCount / config.TerroristRatio - ctCount;

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
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    firstWarden    = true;
    preWardenStats = null;

    var ctArmorValue = getBalance() switch {
      0  => CvArmorEqual.Value,       // Balanced teams
      1  => CvArmorOutnumbered.Value, // Ts outnumber CTs
      -1 => CvArmorOutnumber.Value,   // CTs outnumber Ts
      _  => CvArmorEqual.Value        // default (should never happen)
    };

    /* Round start CT buff */
    foreach (var guardController in Utilities.GetPlayers()
     .Where(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true })) {
      var guardPawn = guardController.PlayerPawn.Value;
      if (guardPawn == null) continue;

      guardPawn.ArmorValue = ctArmorValue;
      Utilities.SetStateChanged(guardPawn, "CCSPlayerPawn", "m_ArmorValue");
    }

    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev,
    GameEventInfo info) {
    if (!((IWardenService)this).IsWarden(ev.Userid)) return HookResult.Continue;

    if (!TryRemoveWarden())
      logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


    notifications.WardenLeft.ToAllChat().ToAllCenter();

    foreach (var player in Utilities.GetPlayers())
      player.ExecuteClientCommand(
        $"play sounds/{config.WardenPassedSoundName}");

    notifications.BecomeNextWarden.ToAllChat();
    return HookResult.Continue;
  }
}