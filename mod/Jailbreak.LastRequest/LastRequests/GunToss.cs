using System.Diagnostics;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Mod.Weapon;
using Microsoft.Extensions.DependencyInjection;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.LastRequest.LastRequests;

public class GunToss(BasePlugin plugin, ILastRequestManager manager,
  IServiceProvider provider, CCSPlayerController prisoner,
  CCSPlayerController guard)
  : TeleportingRequest(plugin, manager, prisoner, guard), IDropListener {
  /// <summary>
  ///   Null if no one has thrown a gun yet, negative if only one has thrown a gun,
  ///   Positive if both have thrown a gun.
  /// </summary>
  private int? bothThrewTick;

  private bool guardTossed, prisonerTossed;
  private CCSWeaponBase? prisonerWeapon, guardWeapon;
  public override LRType Type => LRType.GUN_TOSS;

  // Disabling 500hp LR Safety Net

  // public void OnWeaponDrop(CCSPlayerController player, CCSWeaponBase weapon) {
  //   if (bothThrewTick > 0) return;
  //   if (State != LRState.ACTIVE || !player.IsValid) return;

  //   bothThrewTick = bothThrewTick switch {
  //     null => -Server.TickCount,
  //     < 0  => Server.TickCount,
  //     _    => bothThrewTick
  //   };

  //   if (player == Prisoner) {
  //     prisonerTossed = true;
  //     prisonerWeapon = weapon;
  //   }

  //   if (player == Guard) {
  //     guardTossed = true;
  //     guardWeapon = weapon;
  //   }

  //   if (prisonerTossed && guardTossed) bothThrewTick = Server.TickCount;
    
  //   if (bothThrewTick > 0)
  //     Plugin.AddTimer(5, () => {
  //       if (State != LRState.ACTIVE || !Guard.IsValid || !Guard.Pawn.IsValid) return;
  //       Guard.SetHealth(Math.Min(Guard.Pawn.Value!.Health, 100));
  //       Guard.SetArmor(Math.Min(Guard.PawnArmor, 100));
  //     });
  // }

  public override void Setup() {
    base.Setup();

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();
    
    // Disabling 500hp LR Safety Net

    // Server.NextFrame(() => {
    //   if (!Guard.IsValid) return;

    //   Guard.SetHealth(500);
    //   Guard.SetArmor(500);
    // });

    Plugin.AddTimer(3, Execute);
  }

  public override void Execute() {
    Prisoner.GiveNamedItem("weapon_knife");
    Guard.GiveNamedItem("weapon_knife");
    Prisoner.GiveNamedItem("weapon_deagle");
    Guard.GiveNamedItem("weapon_deagle");
    Prisoner.GetWeaponBase("weapon_deagle").SetAmmo(0, 7);

    Server.RunOnTick(Server.TickCount + 16, () => State = LRState.ACTIVE);
  }

  public override void OnEnd(LRResult result) { State = LRState.COMPLETED; }

  public override bool PreventEquip(CCSPlayerController player,
    CCSWeaponBaseVData weapon) {
    if (State != LRState.ACTIVE) return false;

    if (player.Slot != Prisoner.Slot && player.Slot != Guard.Slot) {
      if (weapon.Name != "weapon_deagle") return false;
      var guardGunDist = guardWeapon == null ?
        float.MaxValue :
        guardWeapon.AbsOrigin!.DistanceSquared(
          Guard.PlayerPawn.Value!.AbsOrigin!);

      var prisonerGunDist = prisonerWeapon == null ?
        float.MaxValue :
        prisonerWeapon.AbsOrigin!.DistanceSquared(Prisoner.PlayerPawn.Value!
         .AbsOrigin!);

      var dist = Math.Min(guardGunDist, prisonerGunDist);
      return dist < 16500;
    }

    if (bothThrewTick is null or < 0) return true;
    var time = Server.TickCount - bothThrewTick.Value;
    return time < 64 * 4;
  }
}