using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class ShotForShot : WeaponizedRequest {
  private readonly ChatMenu chatMenu;
  private string? CHOSEN_PISTOL;
  private CCSPlayerController? whosShot;

  public ShotForShot(BasePlugin plugin, ILastRequestManager manager,
    CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
    manager, prisoner, guard) {
    chatMenu = new ChatMenu("Shot For Shot");
    foreach (var pistol in Tag.PISTOLS) {
      chatMenu.AddMenuOption(pistol.GetFriendlyWeaponName(), OnSelect);
    }
  }

  public override LRType Type => LRType.SHOT_FOR_SHOT;

  private void OnSelect(CCSPlayerController player, ChatMenuOption option) {
    if (player.Slot != Prisoner.Slot) return;
    MenuManager.CloseActiveMenu(player);

    CHOSEN_PISTOL = Tag.PISTOLS.ElementAt(Array.IndexOf(
    [
      "Desert Eagle", "Dualies", "Five Seven", "Glock 18", "HPK2000", "P250",
      "USPS", "Tec9", "CZ75", "Revolver"
    ], option.Text));

    PrintToParticipants(player.PlayerName + " has chosen to use the "
      + CHOSEN_PISTOL.GetFriendlyWeaponName());
    State = LRState.ACTIVE;

    Prisoner.GiveNamedItem(CHOSEN_PISTOL);
    Prisoner.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(0, 0);
    Guard.GiveNamedItem(CHOSEN_PISTOL);
    Guard.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(0, 0);

    whosShot = new Random().Next(2) == 0 ? Prisoner : Guard;
    PrintToParticipants(whosShot.PlayerName
      + " has been chosen to shoot first");

    whosShot.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(1, 0);
  }

  public override void Setup() {
    Plugin.RegisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
    base.Setup();

    CHOSEN_PISTOL = String.Empty;
    chatMenu.Title =
      $"Shot For Shot - {Prisoner.PlayerName} vs {Guard.PlayerName}";
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
        PrintToParticipants("Even health, since " + whosShot!.PlayerName
          + " had the shot last, they lose.");
        result = whosShot.Slot == Prisoner.Slot ?
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
    if (CHOSEN_PISTOL == String.Empty)
      Manager.EndLastRequest(this, LRResult.TIMED_OUT);
  }

  private HookResult OnPlayerShoot(EventPlayerShoot @event,
    GameEventInfo info) {
    if (State != LRState.ACTIVE) return HookResult.Continue;

    var player = @event.Userid;
    if (player == null || whosShot == null || !player.IsReal())
      return HookResult.Continue;

    if (player.Slot != Prisoner.Slot && player.Slot != Guard.Slot)
      return HookResult.Continue;
    if (player.Slot != whosShot.Slot) {
      PrintToParticipants(player.PlayerName + " cheated.");
      player.Pawn.Value?.CommitSuicide(false, true);
      return HookResult.Handled;
    }

    PrintToParticipants(player.PlayerName + " has shot.");
    var opponent = player.Slot == Prisoner.Slot ? Guard : Prisoner;
    opponent.PrintToChat("Your shot");
    opponent.GetWeaponBase(CHOSEN_PISTOL).SetAmmo(1, 0);
    whosShot = opponent;
    return HookResult.Continue;
  }

  public override void OnEnd(LRResult result) {
    Plugin.DeregisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
    State = LRState.COMPLETED;
  }
}