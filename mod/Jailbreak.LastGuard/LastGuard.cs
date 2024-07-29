using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastGuard;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Utils;
using Jailbreak.Validator;
using MStatsShared;

namespace Jailbreak.LastGuard;

public class LastGuard(ILastGuardNotifications notifications,
  ILastRequestManager lrManager) : ILastGuardService, IPluginBehavior {
  public readonly FakeConVar<bool> CvAlwaysOverrideCt = new(
    "css_jb_lg_apply_lower_hp",
    "If true, the LG will be forced lower health if calculated");

  public readonly FakeConVar<double> CvGuardHealthRatio = new(
    "css_jb_lg_ct_hp_ratio", "Ratio of CT : T Health", 0.6,
    ConVarFlags.FCVAR_NONE, new RangeValidator<double>(0.00001, 10));

  public readonly FakeConVar<int> CvLGBaseRoundTime = new("css_jb_lg_time_base",
    "Round time to set when LG is activated, 0 to disable", 60);

  public readonly FakeConVar<int> CvLGKillBonusTime =
    new("css_jb_lg_time_per_kill",
      "Additional round time to add per prisoner kill", 10);

  public readonly FakeConVar<int> CvLGMaxTime = new("css_jb_lg_time_max",
    "Max round time to give the LG regardless of bonuses", 120);

  public readonly FakeConVar<int> CvLGPerPrisonerTime =
    new("css_jb_lg_time_per_prisoner",
      "Additional round time to add per prisoner", 10);

  public readonly FakeConVar<string> CvLGWeapon = new("css_jb_lg_t_weapon",
    "Weapon to give remaining prisoners once LG activates", "",
    ConVarFlags.FCVAR_NONE, new WeaponValidator());

  public readonly FakeConVar<int> CvMaxCtHealth = new("css_jb_lg_max_hp",
    "Max HP that the LG can have otherwise", 125, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 1000));

  public readonly FakeConVar<int> CvMaxTHealthContribution = new(
    "css_jb_lg_t_max_hp", "Max HP to contribute per T to LG", 200,
    ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1000));

  public readonly FakeConVar<int> CvMinimumCts = new("css_jb_lg_min_cts",
    "Minimum number of CTs to start last guard", 2, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 32));

  private readonly Random rng = new();
  private bool canStart;
  private bool isLastGuard;
  private List<CCSPlayerController> lastGuardPrisoners = [];

  public int CalculateHealth() {
    var aliveTerrorists = Utilities.GetPlayers()
     .Where(plr => plr is { PawnIsAlive: true, Team: CsTeam.Terrorist })
     .ToList();

    return (int)Math.Floor(aliveTerrorists
     .Select(player => player.PlayerPawn.Value?.Health ?? 0)
     .Select(playerHealth
        => Math.Min(playerHealth, CvMaxTHealthContribution.Value))
     .Sum() * CvGuardHealthRatio.Value);
  }

  public void StartLastGuard(CCSPlayerController lastGuard) {
    var guardPlayerPawn = lastGuard.PlayerPawn.Value;

    if (guardPlayerPawn == null || !guardPlayerPawn.IsValid) return;

    isLastGuard = true;

    API.Stats?.PushStat(new ServerStat("JB_LASTGUARD",
      lastGuard.SteamID.ToString()));

    var calculated = CalculateHealth();

    if (calculated < guardPlayerPawn.Health && !CvAlwaysOverrideCt.Value) {
      if (guardPlayerPawn.Health > CvMaxCtHealth.Value)
        guardPlayerPawn.Health = CvMaxCtHealth.Value;
    } else { guardPlayerPawn.Health = calculated; }

    Utilities.SetStateChanged(guardPlayerPawn, "CBaseEntity", "m_iHealth");

    // foreach (var player in Utilities.GetPlayers().Where(p => p.IsReal()))
    //     player.ExecuteClientCommand("play sounds/lastct");

    lastGuardPrisoners = Utilities.GetPlayers()
     .Where(p => p is { PawnIsAlive: true, Team: CsTeam.Terrorist })
     .ToList();

    if (CvLGBaseRoundTime.Value != 0)
      RoundUtil.SetTimeRemaining(Math.Min(CvLGBaseRoundTime.Value,
        CvLGMaxTime.Value));
    addRoundTimeCapped(CvLGPerPrisonerTime.Value * lastGuardPrisoners.Count,
      CvLGMaxTime.Value);

    var prisonerHp =
      lastGuardPrisoners.Sum(prisoner
        => prisoner.PlayerPawn.Value?.Health ?? 0);

    notifications.LGStarted(lastGuard, guardPlayerPawn.Health, prisonerHp)
     .ToAllCenter()
     .ToAllChat();

    if (string.IsNullOrEmpty(CvLGWeapon.Value)) return;

    foreach (var player in lastGuardPrisoners)
      player.GiveNamedItem(CvLGWeapon.Value);
  }

  [GameEventHandler]
  public HookResult OnPlayerDeathEvent(EventPlayerDeath @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;
    checkLastGuard(@event.Userid);

    if (!isLastGuard) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;

    addRoundTimeCapped(CvLGKillBonusTime.Value, CvLGMaxTime.Value);

    giveGun(player);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    checkLastGuard(@event.Userid);
    return HookResult.Continue;
  }

  private void giveGun(CCSPlayerController poi) {
    lastGuardPrisoners = lastGuardPrisoners.Where(p
        => p is { IsValid: true, PawnIsAlive: true } && poi.Index != p.Index
        && !playerHasGun(p))
     .ToList();
    if (lastGuardPrisoners.Count == 0) return;

    var random = lastGuardPrisoners[rng.Next(lastGuardPrisoners.Count)];
    random.GiveNamedItem("weapon_glock");
    lastGuardPrisoners.Remove(random);
  }

  private bool playerHasGun(CCSPlayerController player) {
    var weapons = player.Pawn.Value?.WeaponServices;
    if (weapons == null) return false;
    foreach (var weapon in weapons.MyWeapons) {
      if (weapon.Value == null) continue;
      if (!Tag.GUNS.Contains(weapon.Value.DesignerName)) continue;
      return true;
    }

    return false;
  }

  private void checkLastGuard(CCSPlayerController? poi) {
    if (poi == null) return;
    if (isLastGuard) return;
    lastGuardPrisoners.Remove(poi);
    if (poi.Team != CsTeam.CounterTerrorist) return;
    var aliveCts = Utilities.GetPlayers()
     .Count(plr => plr.SteamID != poi.SteamID && plr is {
        PawnIsAlive: true, Team: CsTeam.CounterTerrorist
      });

    if (aliveCts != 1 || lrManager.IsLREnabled) return;
    var lastGuard = Utilities.GetPlayers()
     .First(plr => plr != poi && plr is {
        PawnIsAlive: true, Team: CsTeam.CounterTerrorist
      });

    if (canStart) StartLastGuard(lastGuard);
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    isLastGuard = false;
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStartEvent(EventRoundStart @event,
    GameEventInfo info) {
    canStart = Utilities.GetPlayers()
       .Count(plr
          => plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist })
      >= CvMinimumCts.Value;
    return HookResult.Continue;
  }

  private void addRoundTimeCapped(int time, int max) {
    var timeleft                    = RoundUtil.GetTimeRemaining();
    if (timeleft + time > max) time = max - timeleft;
    RoundUtil.AddTimeRemaining(time);
  }
}