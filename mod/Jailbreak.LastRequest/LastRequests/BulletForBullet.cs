using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest.LastRequests;

public class BulletForBullet : TeleportingRequest {
  private static readonly string[] GUNS = [
    "weapon_deagle", "weapon_usp_silencer", "weapon_tec9", "weapon_ssg08",
    "weapon_nova"
  ];

  private static readonly string[] GUN_NAMES = [
    "Desert Eagle", "USP-S", "Tec9", "Scout", "Nova"
  ];

  private readonly ChatMenu chatMenu;
  private readonly bool magForMag;
  private readonly ILRB4BLocale msg;
  private string? CHOSEN_PISTOL;
  private int? whosShot, magSize;

  public BulletForBullet(BasePlugin plugin, IServiceProvider provider,
    CCSPlayerController prisoner, CCSPlayerController guard,
    bool magForMag) : base(plugin,
    provider.GetRequiredService<ILastRequestManager>(), prisoner, guard) {
    this.magForMag = magForMag;
    chatMenu       = new ChatMenu(magForMag ? "Mag for Mag" : "Shot for Shot");
    foreach (var pistol in GUNS)
      chatMenu.AddMenuOption(pistol.GetFriendlyWeaponName(), OnSelect);

    msg = provider.GetRequiredService<ILRB4BLocale>();
  }

  public override LRType Type
    => magForMag ? LRType.MAG_FOR_MAG : LRType.SHOT_FOR_SHOT;

  private void OnSelect(CCSPlayerController player, ChatMenuOption option) {
    if (player.Slot != Prisoner.Slot) return;
    MenuManager.CloseActiveMenu(player);

    CHOSEN_PISTOL = GUNS[Array.IndexOf(GUN_NAMES, option.Text)];

    msg.WeaponSelected(player, CHOSEN_PISTOL).ToChat(Prisoner, Guard);

    State = LRState.ACTIVE;

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();
    Prisoner.GiveNamedItem(CHOSEN_PISTOL);
    Guard.GiveNamedItem(CHOSEN_PISTOL);

    Plugin.AddTimer(0.5f, () => {
      magSize = (magForMag ?
        Prisoner.GetWeaponBase(CHOSEN_PISTOL)!.VData?.MaxClip1 :
        1) ?? 1;
      Prisoner.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(0, 0);
      Guard.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(0, 0);
      var shooter = new Random().Next(2) == 0 ? Prisoner : Guard;
      whosShot = shooter.Slot;
      msg.PlayerGoesFirst(shooter).ToChat(Prisoner, Guard);

      shooter.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(magSize.Value, 0);
    });
  }

  public override void Setup() {
    Plugin.RegisterEventHandler<EventBulletImpact>(OnPlayerShoot);

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();

    base.Setup();
    Execute();

    CHOSEN_PISTOL = string.Empty;
    chatMenu.Title =
      $"{chatMenu.Title} - {Prisoner.PlayerName} vs {Guard.PlayerName}";
  }


  public override void Execute() {
    State = LRState.PENDING;
    MenuManager.OpenChatMenu(Prisoner, chatMenu);

    Plugin.AddTimer(10, timeout, TimerFlags.STOP_ON_MAPCHANGE);

    Plugin.AddTimer(30, () => {
      if (State != LRState.ACTIVE) return;
      Prisoner.GiveNamedItem("weapon_knife");
      Guard.GiveNamedItem("weapon_knife");
    });
    Plugin.AddTimer(60, () => {
      if (State != LRState.ACTIVE) return;
      var result = Guard.Health > Prisoner.Health ?
        LRResult.GUARD_WIN :
        LRResult.PRISONER_WIN;
      if (Guard.Health == Prisoner.Health) {
        var winner = whosShot != Prisoner.Slot ? Prisoner : Guard;
        msg.WinByReason(winner, "equal health");

        result = whosShot == Prisoner.Slot ?
          LRResult.GUARD_WIN :
          LRResult.PRISONER_WIN;
      } else {
        msg.WinByHealth(result == LRResult.GUARD_WIN ? Guard : Prisoner);
      }

      if (result == LRResult.GUARD_WIN)
        Prisoner.Pawn.Value?.CommitSuicide(false, true);
      else
        Guard.Pawn.Value?.CommitSuicide(false, true);
    }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void timeout() {
    if (CHOSEN_PISTOL == string.Empty)
      Manager.EndLastRequest(this, LRResult.TIMED_OUT);
  }

  private HookResult OnPlayerShoot(EventBulletImpact @event,
    GameEventInfo info) {
    if (State != LRState.ACTIVE) return HookResult.Continue;

    var player = @event.Userid;
    if (player == null || whosShot == null || !player.IsValid
      || magSize == null)
      return HookResult.Continue;

    if (player.Slot != Prisoner.Slot && player.Slot != Guard.Slot)
      return HookResult.Continue;
    if (player.Slot != whosShot) {
      PrintToParticipants(player.PlayerName + " cheated.");
      player.Pawn.Value?.CommitSuicide(false, true);
      return HookResult.Handled;
    }

    var bullets = player.GetWeaponBase(CHOSEN_PISTOL!)?.Clip1 ?? 1;
    if (bullets > 1) return HookResult.Continue;

    Server.NextFrame(() => {
      var opponent = player.Slot == Prisoner.Slot ? Guard : Prisoner;
      whosShot = opponent.Slot;
      opponent.GetWeaponBase(CHOSEN_PISTOL!)?.SetAmmo(magSize.Value, 0);
    });
    return HookResult.Continue;
  }

  public override void OnEnd(LRResult result) {
    Plugin.DeregisterEventHandler<EventBulletImpact>(OnPlayerShoot);
    State = LRState.COMPLETED;
  }
}