using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Services;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Damage;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;
using MStatsShared;

namespace Jailbreak.LastRequest;

public class LastRequestManager(ILRLocale messages, IServiceProvider provider)
  : ILastRequestManager, IDamageBlocker {
  public static readonly FakeConVar<int> CV_LR_BASE_TIME =
    new("css_jb_lr_time_base",
      "Round time to set when LR is activated, 0 to disable", 60);

  public static readonly FakeConVar<int> CV_LR_BONUS_TIME =
    new("css_jb_lr_time_per_lr",
      "Additional round time to add per LR completion", 30);

  public static readonly FakeConVar<int> CV_LR_GUARD_TIME =
    new("css_jb_lr_time_per_guard", "Additional round time to add per guard");

  public static readonly FakeConVar<int> CV_PRISONER_TO_LR =
    new("css_jb_lr_activate_lr_at", "Number of prisoners to activate LR at", 2,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 32));

  private ILastRequestFactory? factory;
  public bool IsLREnabledForRound { get; set; } = true;

  public bool ShouldBlockDamage(CCSPlayerController player,
    CCSPlayerController? attacker, EventPlayerHurt @event) {
    if (!IsLREnabled) return false;

    if (attacker == null || !attacker.IsReal()) return false;

    var playerLR   = ((ILastRequestManager)this).GetActiveLR(player);
    var attackerLR = ((ILastRequestManager)this).GetActiveLR(attacker);

    if (playerLR == null && attackerLR == null)
      // Neither of them is in an LR
      return false;

    if (playerLR == null != (attackerLR == null)) {
      // One of them is in an LR
      messages.DamageBlockedInsideLastRequest.ToCenter(attacker);
      return true;
    }

    // Both of them are in LR, verify they're in same LR
    if (playerLR == null) return false;

    if (playerLR.Prisoner.Equals(attacker) || playerLR.Guard.Equals(attacker))
      // Same LR, allow damage
      return false;

    messages.DamageBlockedNotInSameLR.ToCenter(attacker);
    return true;
  }

  public void Start(BasePlugin basePlugin) {
    factory = provider.GetRequiredService<ILastRequestFactory>();
  }

  public bool IsLREnabled { get; set; }

  public IList<AbstractLastRequest> ActiveLRs { get; } =
    new List<AbstractLastRequest>();


  public void DisableLR() { IsLREnabled = false; }

  public void DisableLRForRound() {
    DisableLR();
    IsLREnabledForRound = false;
  }

  public void EnableLR(CCSPlayerController? died = null) {
    messages.LastRequestEnabled().ToAllChat();
    IsLREnabled = true;

    API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST_ACTIVATED"));

    var cts = Utilities.GetPlayers()
     .Count(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true });

    if (CV_LR_BASE_TIME.Value != 0)
      RoundUtil.SetTimeRemaining(CV_LR_BASE_TIME.Value);

    RoundUtil.AddTimeRemaining(CV_LR_GUARD_TIME.Value * cts);

    foreach (var player in Utilities.GetPlayers()) {
      player.ExecuteClientCommand("play sounds/lr");
      if (player.Team != CsTeam.Terrorist || !player.PawnIsAlive) continue;
      if (died != null && player.SteamID == died.SteamID) continue;
      player.ExecuteClientCommandFromServer("css_lr");
    }

    if (API.Gangs != null) {
      var eco = API.Gangs.Services.GetService<IEcoManager>();
      if (eco == null) return;
      var survivors = Utilities.GetPlayers()
       .Where(p => p is { IsBot: false, PawnIsAlive: true })
       .Select(p => new PlayerWrapper(p))
       .ToList();
      Task.Run(async () => {
        foreach (var survivor in survivors)
          await eco.Grant(survivor, 100, reason: "LR Reached");
      });
    }
  }

  public bool InitiateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type) {
    var lr = factory!.CreateLastRequest(prisoner, guard, type);
    lr.Setup();
    ActiveLRs.Add(lr);

    API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST",
      $"{prisoner.SteamID} {type.ToFriendlyString()}"));

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
      RoundUtil.AddTimeRemaining(CV_LR_BONUS_TIME.Value);
      messages.LastRequestDecided(lr, result).ToAllChat();

      if (API.Gangs != null) {
        var eco = API.Gangs.Services.GetService<IEcoManager>();
        if (eco == null) return false;
        var wrapper =
          new PlayerWrapper(result == LRResult.GUARD_WIN ?
            lr.Guard :
            lr.Prisoner);
        Task.Run(async () => {
          await eco.Grant(wrapper, 30, reason: "LR Win");
        });
      }
    }

    API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST_RESULT",
      $"{lr.Prisoner.SteamID} {result.ToString()}"));

    lr.OnEnd(result);
    ActiveLRs.Remove(lr);
    return true;
  }

  [GameEventHandler(HookMode.Pre)]
  public HookResult OnTakeDamage(EventPlayerHurt @event, GameEventInfo info) {
    IDamageBlocker damageBlockerHandler = this;
    return damageBlockerHandler.BlockUserDamage(@event, info);
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
    IsLREnabledForRound = true;
    IsLREnabled         = false;
    foreach (var player in Utilities.GetPlayers())
      MenuManager.CloseActiveMenu(player);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal() || RoundUtil.IsWarmup())
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

    if (!IsLREnabledForRound) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;
    checkLR();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    var player = @event.Userid;

    if (player == null) return HookResult.Continue;

    if (!player.IsReal() || RoundUtil.IsWarmup()) return HookResult.Continue;

    if (IsLREnabled) {
      var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
      if (activeLr != null)
        EndLastRequest(activeLr,
          player.Team == CsTeam.Terrorist ?
            LRResult.GUARD_WIN :
            LRResult.PRISONER_WIN);

      return HookResult.Continue;
    }

    if (!IsLREnabledForRound) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;

    checkLR();
    return HookResult.Continue;
  }

  private void checkLR() {
    Server.RunOnTick(Server.TickCount + 32, () => {
      if (IsLREnabled) return;
      if (Utilities.GetPlayers().All(p => p.Team != CsTeam.CounterTerrorist))
        return;
      if (countAlivePrisoners() > CV_PRISONER_TO_LR.Value) return;
      EnableLR();
    });
  }

  private int countAlivePrisoners() {
    return Utilities.GetPlayers().Count(prisonerCountsToLR);
  }

  private bool prisonerCountsToLR(CCSPlayerController player) {
    if (!player.IsReal()) return false;
    if (!player.PawnIsAlive) return false;
    return player.Team == CsTeam.Terrorist;
  }
}