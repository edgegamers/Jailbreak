using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Services;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Rebel;

public class RebelListener(IRebelService rebelService,
  ILastRequestManager lastRequestManager) : IPluginBehavior {
  [GameEventHandler]
  public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    if (player.Team != CsTeam.CounterTerrorist) return HookResult.Continue;

    var attacker = @event.Attacker;
    if (attacker == null || !attacker.IsReal()) return HookResult.Continue;

    if (attacker.Team != CsTeam.Terrorist) return HookResult.Continue;

    if (lastRequestManager.IsInLR(attacker)) return HookResult.Continue;

    rebelService.MarkRebel(attacker);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) {
    var player = ev.Userid;
    if (player == null || !player.IsValid || player.IsBot)
      return HookResult.Continue;
    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;
    if (!rebelService.IsRebel(player)) return HookResult.Continue;

    var eco = API.Gangs?.Services.GetService<IEcoManager>();

    if (eco == null) return HookResult.Continue;

    var wrapper = new PlayerWrapper(player);
    var hasGun  = playerHasGun(player);

    Task.Run(async ()
      => await eco.Grant(wrapper, hasGun ? 20 : 10, true, "Rebel Kill"));
    return HookResult.Continue;
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
}