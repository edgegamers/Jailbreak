using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest.LastRequests;

public class NoScope(BasePlugin plugin, IServiceProvider provider,
  CCSPlayerController prisoner, CCSPlayerController guard) : WeaponizedRequest(
    plugin, provider.GetRequiredService<IServiceProvider>(), prisoner, guard),
  ILastRequestConfig {
  public override LRType Type => LRType.NO_SCOPE;
  public bool RequiresConfiguration => true;

  private string weaponName = "weapon_ssg08";

  public override void Setup() {
    base.Setup();

    Plugin.RegisterListener<Listeners.OnTick>(OnTick);
  }

  public void OpenConfigMenu(CCSPlayerController prisoner,
    CCSPlayerController guard, Action onComplete) {
    var menu = new CenterHtmlMenu("Choose Pistol", Plugin);

    addWeaponOption(menu, "Scout", "weapon_ssg08", onComplete);
    addWeaponOption(menu, "AWP", "weapon_awp", onComplete);
    addWeaponOption(menu, "SCAR-20", "weapon_scar20", onComplete);
    addWeaponOption(menu, "G3SG1", "weapon_g3sg1", onComplete);

    MenuManager.OpenCenterHtmlMenu(Plugin, prisoner, menu);
  }

  private void addWeaponOption(CenterHtmlMenu menu, string displayName,
    string weapon, Action onComplete) {
    menu.AddMenuOption(displayName, (player, option) => {
      weaponName = weapon;
      MenuManager.CloseActiveMenu(player);
      onComplete();
    });
  }

  private void OnTick() {
    if (State != LRState.ACTIVE) return;

    if (!Prisoner.IsReal() || !Guard.IsReal()) return;

    if (Prisoner.PlayerPawn.Value == null || Guard.PlayerPawn.Value == null)
      return;
    disableScope(Prisoner);
    disableScope(Guard);
  }

  private void disableScope(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
  }

  public override void Execute() {
    PrintToParticipants("Go!");
    Prisoner.GiveNamedItem("weapon_ssg08");
    Guard.GiveNamedItem("weapon_ssg08");
    State = LRState.ACTIVE;

    Plugin.AddTimer(30, () => {
      if (State != LRState.ACTIVE) return;
      Prisoner.GiveNamedItem("weapon_knife");
      Guard.GiveNamedItem("weapon_knife");
    }, TimerFlags.STOP_ON_MAPCHANGE);

    Plugin.AddTimer(60, () => {
      if (State != LRState.ACTIVE) return;

      Manager.EndLastRequest(this,
        Guard.Health > Prisoner.Health ?
          LRResult.GUARD_WIN :
          LRResult.PRISONER_WIN);
    }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  public override void OnEnd(LRResult result) {
    State = LRState.COMPLETED;
    Plugin.RemoveListener<Listeners.OnTick>(OnTick);

    if (result != LRResult.GUARD_WIN && result != LRResult.PRISONER_WIN) return;

    var winner = result == LRResult.GUARD_WIN ? Guard : Prisoner;

    winner.RemoveWeapons();
    winner.GiveNamedItem("weapon_knife");
    winner.GiveNamedItem("weapon_ak47");
  }

  public override bool PreventEquip(CCSPlayerController player,
    CCSWeaponBaseVData weapon) {
    if (State == LRState.PENDING) return false;
    if (player != Prisoner && player != Guard) return false;
    return !weapon.Name.Contains("knife") && !weapon.Name.Contains("bayonet");
  }
}