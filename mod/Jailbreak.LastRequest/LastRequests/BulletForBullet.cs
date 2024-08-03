using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class BulletForBullet : TeleportingRequest {
  private static readonly string[] GUNS = [
    "weapon_deagle", "weapon_glock", "weapon_hkp2000", "weapon_tec9",
    "weapon_cz75a"
  ];

  private static readonly string[] GUN_NAMES = [
    "Desert Eagle", "Glock 18", "USP-S", "Tec9", "CZ75"
  ];

  private readonly ChatMenu chatMenu;
  private string? CHOSEN_PISTOL;
  private readonly bool magForMag;
  private int? whosShot, magSize;

  public BulletForBullet(BasePlugin plugin, ILastRequestManager manager,
    CCSPlayerController prisoner, CCSPlayerController guard,
    bool magForMag) : base(plugin, manager, prisoner, guard) {
    this.magForMag = magForMag;
    chatMenu       = new ChatMenu(magForMag ? "Mag for Mag" : "Shot for Shot");
    foreach (var pistol in GUNS)
      chatMenu.AddMenuOption(pistol.GetFriendlyWeaponName(), OnSelect);
  }

  public override LRType Type => LRType.SHOT_FOR_SHOT;

  private void OnSelect(CCSPlayerController player, ChatMenuOption option) {
    if (player.Slot != Prisoner.Slot) return;
    MenuManager.CloseActiveMenu(player);

    CHOSEN_PISTOL = GUNS[Array.IndexOf(GUN_NAMES, option.Text)];

    PrintToParticipants(player.PlayerName + " chose the "
      + CHOSEN_PISTOL.GetFriendlyWeaponName());
    State = LRState.ACTIVE;

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();
    Prisoner.GiveNamedItem(CHOSEN_PISTOL);
    Guard.GiveNamedItem(CHOSEN_PISTOL);

    Server.NextFrame(() => {
      magSize = magForMag ?
        Prisoner.GetWeaponBase(CHOSEN_PISTOL)!.VData!.MaxClip1 :
        1;
      Prisoner.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(0, 0);
      Guard.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(0, 0);
      //steal the VData of the prisoners gun for mag size

      var shooter = new Random().Next(2) == 0 ? Prisoner : Guard;
      whosShot = shooter.Slot;
      PrintToParticipants(shooter.PlayerName
        + $" gets to shoot first {magSize} {magForMag}");

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
      PrintToParticipants("Time's Up!");
      var result = Guard.Health > Prisoner.Health ?
        LRResult.GUARD_WIN :
        LRResult.PRISONER_WIN;
      if (Guard.Health == Prisoner.Health) {
        var active = whosShot == Prisoner.Slot ? Prisoner : Guard;
        PrintToParticipants("Even health, since " + active.PlayerName
          + " had the shot, they lose.");
        result = whosShot == Prisoner.Slot ?
          LRResult.GUARD_WIN :
          LRResult.PRISONER_WIN;
      } else { PrintToParticipants("Health was the deciding factor. "); }

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
      opponent.GetWeaponBase(CHOSEN_PISTOL!).SetAmmo(magSize.Value, 0);
    });
    return HookResult.Continue;
  }

  public override void OnEnd(LRResult result) {
    Plugin.DeregisterEventHandler<EventBulletImpact>(OnPlayerShoot);
    State = LRState.COMPLETED;
  }
}