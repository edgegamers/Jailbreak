using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Damage;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public class LastRequestManager(ILastRequestMessages messages,
  IServiceProvider provider) : ILastRequestManager, IBlockUserDamage {
  private ILastRequestFactory? factory;

  public readonly FakeConVar<int> CvPrisonerToLR =
    new("css_jb_lr_activate_lr_at", "Number of prisoners to activate LR at", 2,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 32));

  public readonly FakeConVar<int> CvLRBaseRoundTime = new("css_jb_lr_time_base",
    "Round time to set when LR is activated, 0 to disable", 60);

  public readonly FakeConVar<int> CvLRGuardTime =
    new("css_jb_lr_time_per_guard", "Additional round time to add per guard");

  public readonly FakeConVar<int> CvLRBonusTime = new("css_jb_lr_time_per_lr",
    "Additional round time to add per LR completion", 30);

  public void Start(BasePlugin basePlugin) {
    factory = provider.GetRequiredService<ILastRequestFactory>();
  }

  public bool ShouldBlockDamage(CCSPlayerController player,
    CCSPlayerController? attacker, EventPlayerHurt @event) {
    if (!IsLREnabled) return false;

    if (attacker == null || !attacker.IsReal()) return false;

    var playerLR   = ((ILastRequestManager)this).GetActiveLR(player);
    var attackerLR = ((ILastRequestManager)this).GetActiveLR(attacker);

    if (playerLR == null && attackerLR == null)
      // Neither of them is in an LR
      return false;

    if ((playerLR == null) != (attackerLR == null)) {
      // One of them is in an LR
      messages.DamageBlockedInsideLastRequest.ToPlayerCenter(attacker);
      return true;
    }

    // Both of them are in LR, verify they're in same LR
    if (playerLR == null) return false;

    if (playerLR.Prisoner.Equals(attacker) || playerLR.Guard.Equals(attacker))
      // Same LR, allow damage
      return false;

    messages.DamageBlockedNotInSameLR.ToPlayerCenter(attacker);
    return true;
  }

  public bool IsLREnabled { get; set; }

  public IList<AbstractLastRequest> ActiveLRs { get; } =
    new List<AbstractLastRequest>();


  public void DisableLR() { IsLREnabled = false; }

  public void EnableLR(CCSPlayerController? died = null) {
    messages.LastRequestEnabled().ToAllChat();
    IsLREnabled = true;

    var cts = Utilities.GetPlayers()
     .Count(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true });

    if (CvLRBaseRoundTime.Value != 0) setRoundTime(CvLRBaseRoundTime.Value);

    addRoundTime(CvLRGuardTime.Value * cts);

    foreach (var player in Utilities.GetPlayers()) {
      // player.ExecuteClientCommand($"play sounds/lr");
      if (player.Team != CsTeam.Terrorist || !player.PawnIsAlive) continue;
      if (died != null && player.SteamID == died.SteamID) continue;
      player.ExecuteClientCommandFromServer("css_lr");
    }
  }

  public bool InitiateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type) {
    var lr = factory!.CreateLastRequest(prisoner, guard, type);
    lr.Setup();
    ActiveLRs.Add(lr);

    if (prisoner.Pawn.Value != null) {
      prisoner.Pawn.Value.Health            = 100;
      prisoner.PlayerPawn.Value!.ArmorValue = 0;
      Utilities.SetStateChanged(prisoner.Pawn.Value, "CBaseEntity",
        "m_iHealth");
    }

    if (guard.Pawn.Value != null) {
      guard.Pawn.Value.Health            = 100;
      guard.PlayerPawn.Value!.ArmorValue = 0;
      Utilities.SetStateChanged(guard.Pawn.Value, "CBaseEntity", "m_iHealth");
    }

    messages.InformLastRequest(lr).ToAllChat();
    return true;
  }

  public bool EndLastRequest(AbstractLastRequest lr, LRResult result) {
    if (result is LRResult.GUARD_WIN or LRResult.PRISONER_WIN) {
      addRoundTime(CvLRBonusTime.Value);
      messages.LastRequestDecided(lr, result).ToAllChat();
    }

    lr.OnEnd(result);
    ActiveLRs.Remove(lr);
    return true;
  }

  [GameEventHandler(HookMode.Pre)]
  public HookResult OnTakeDamage(EventPlayerHurt @event, GameEventInfo info) {
    IBlockUserDamage damageHandler = this;
    return damageHandler.BlockUserDamage(@event, info);
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var lr in ActiveLRs.ToList())
      EndLastRequest(lr, LRResult.TIMED_OUT);

    IsLREnabled = false;
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    foreach (var player in Utilities.GetPlayers())
      MenuManager.CloseActiveMenu(player);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()
      || ServerExtensions.GetGameRules().WarmupPeriod)
      return HookResult.Continue;

    if (IsLREnabled) {
      // Handle active LRs
      var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
      if (activeLr == null || activeLr.State == LRState.COMPLETED)
        return HookResult.Continue;
      var isPrisoner = activeLr.Prisoner.Slot == player.Slot;
      EndLastRequest(activeLr,
        isPrisoner ? LRResult.GUARD_WIN : LRResult.PRISONER_WIN);

      return HookResult.Continue;
    }

    if (player.GetTeam() != CsTeam.Terrorist) return HookResult.Continue;

    if (countAlivePrisoners() - 1 > CvPrisonerToLR.Value)
      return HookResult.Continue;

    if (Utilities.GetPlayers().All(p => p.Team != CsTeam.CounterTerrorist))
      return HookResult.Continue;

    EnableLR(player);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    var player = @event.Userid;

    if (player == null) return HookResult.Continue;

    if (!player.IsReal() || ServerExtensions.GetGameRules().WarmupPeriod)
      return HookResult.Continue;

    if (IsLREnabled) {
      var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
      if (activeLr != null) {
        EndLastRequest(activeLr,
          player.Team == CsTeam.Terrorist ?
            LRResult.GUARD_WIN :
            LRResult.PRISONER_WIN);
      }

      return HookResult.Continue;
    }

    if (player.GetTeam() != CsTeam.Terrorist) return HookResult.Continue;
    if (countAlivePrisoners() > CvPrisonerToLR.Value)
      return HookResult.Continue;

    EnableLR();
    return HookResult.Continue;
  }

  private int getCurrentTimeElapsed() {
    var gamerules  = ServerExtensions.GetGameRules();
    var freezeTime = gamerules.FreezeTime;
    return (int)(Server.CurrentTime - gamerules.RoundStartTime - freezeTime);
  }

  private void setRoundTime(int time) {
    var gamerules = ServerExtensions.GetGameRules();
    gamerules.RoundTime = getCurrentTimeElapsed() + time;

    Utilities.SetStateChanged(ServerExtensions.GetGameRulesProxy(),
      "CCSGameRulesProxy", "m_pGameRules");
  }

  private void addRoundTime(int time) {
    var gamerules = ServerExtensions.GetGameRules();
    gamerules.RoundTime += time;

    Utilities.SetStateChanged(ServerExtensions.GetGameRulesProxy(),
      "CCSGameRulesProxy", "m_pGameRules");
  }

  private int countAlivePrisoners() {
    return Utilities.GetPlayers().Count(prisonerCountsToLR);
  }

  private bool prisonerCountsToLR(CCSPlayerController player) {
    if (!player.IsReal()) return false;
    if (!player.PawnIsAlive) return false;
    return player.GetTeam() == CsTeam.Terrorist;
  }
}