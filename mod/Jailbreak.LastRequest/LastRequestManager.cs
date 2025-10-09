using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl.Extensions;
using Gangs.BaseImpl.Stats;
using Gangs.LastRequestColorPerk;
using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Services;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Damage;
using Jailbreak.Public.Mod.LastGuard;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Mod.Rainbow;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Weapon;
using Jailbreak.Public.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MStatsShared;

namespace Jailbreak.LastRequest;
//TODO: Fix Various Server Crashes

public class LastRequestManager(ILRLocale messages, IServiceProvider provider)
  : ILastRequestManager, IDamageBlocker {
  public static readonly FakeConVar<int> CV_LR_BASE_TIME =
    new("css_jb_lr_time_base",
      "Round time to set when LR is activated, 0 to disable", 60);

  public static readonly FakeConVar<int> CV_LR_BONUS_TIME =
    new("css_jb_lr_time_per_lr",
      "Additional round time to add per LR completion", 20);

  public static readonly FakeConVar<int> CV_LR_GUARD_TIME =
    new("css_jb_lr_time_per_guard", "Additional round time to add per guard");

  public static readonly FakeConVar<int> CV_PRISONER_TO_LR =
    new("css_jb_lr_activate_lr_at", "Number of prisoners to activate LR at", 2,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 32));

  public static readonly FakeConVar<int> CV_MIN_PLAYERS_FOR_CREDITS =
    new("css_jb_min_players_for_credits",
      "Minimum number of players to start giving credits out", 5);

  public static readonly FakeConVar<int> CV_MAX_TIME_FOR_LR =
    new("css_jb_max_time_for_lr", "Maximum round time during LR", 60);

  private readonly IRainbowColorizer rainbowColorizer =
    provider.GetRequiredService<IRainbowColorizer>();

  private ILastRequestFactory? factory;
  public bool IsLREnabledForRound { get; set; } = true;

  public bool ShouldBlockDamage(CCSPlayerController victim,
    CCSPlayerController? attacker) {
    if (!IsLREnabled) return false;

    if (attacker == null || !attacker.IsReal()) return false;

    var victimLR   = ((ILastRequestManager)this).GetActiveLR(victim);
    var attackerLR = ((ILastRequestManager)this).GetActiveLR(attacker);

    if (victimLR == null && attackerLR == null)
      // Neither of them is in an LR
      return false;

    if (victimLR == null != (attackerLR == null)) {
      // One of them is in an LR
      messages.DamageBlockedInsideLastRequest.ToCenter(attacker);
      return true;
    }

    // Both of them are in LR, verify they're in same LR
    if (victimLR == null) return false;

    if (victimLR.Prisoner.SteamID == attacker.SteamID
        || victimLR.Guard.SteamID == attacker.SteamID)
      // The person attacking is the victim's LR participant, allow damage
      return false;

    messages.DamageBlockedNotInSameLR.ToCenter(attacker);
    return true;
  }

  public bool IsLREnabled { get; set; }

  public IList<AbstractLastRequest> ActiveLRs { get; } =
    new List<AbstractLastRequest>();

  public void Start(BasePlugin basePlugin) {
    factory = provider.GetRequiredService<ILastRequestFactory>();

    if (API.Gangs == null) return;

    var stats = API.Gangs.Services.GetService<IStatManager>();
    stats?.Stats.Add(new LRStat());

    basePlugin.RegisterListener<Listeners.OnEntityParentChanged>(OnDrop);
    VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage,
      HookMode.Pre);
  }

  public void Dispose() {
    VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage,
      HookMode.Pre);
  }

  public void DisableLR() { IsLREnabled = false; }

  public void DisableLRForRound() {
    DisableLR();
    IsLREnabledForRound = false;
  }

  public void EnableLR(CCSPlayerController? died = null) {
    messages.LastRequestEnabled().ToAllChat();
    IsLREnabled = true;

    var cts = Utilities.GetPlayers()
     .Count(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true });

    if (CV_LR_BASE_TIME.Value != 0)
      RoundUtil.SetTimeRemaining(CV_LR_BASE_TIME.Value);

    RoundUtil.AddTimeRemaining(CV_LR_GUARD_TIME.Value * cts);

    var players   = Utilities.GetPlayers();
    var lastGuard = provider.GetService<ILastGuardService>();
    var rebel     = provider.GetService<IRebelService>();
    foreach (var player in players) {
      player.ExecuteClientCommand("play sounds/lr");
      var wrapper = new PlayerWrapper(player);

      if (!player.PawnIsAlive) continue;

      if (API.Gangs != null) {
        var playerStatMgr = API.Gangs.Services.GetService<IPlayerStatManager>();
        if (playerStatMgr != null)
          Task.Run(async () => {
            var stat =
              await playerStatMgr.GetForPlayer<LRData>(wrapper, LRStat.STAT_ID)
              ?? new LRData();
            if (wrapper.Team == CsTeam.Terrorist)
              stat.TLrs++;
            else
              stat.CtLrs++;

            await playerStatMgr.SetForPlayer(wrapper, LRStat.STAT_ID, stat);
          });
      }

      if (player.Team != CsTeam.Terrorist) continue;
      if (died != null && player.SteamID == died.SteamID) continue;

      if (lastGuard is { IsLastGuardActive: true }) rebel?.UnmarkRebel(player);
      player.ExecuteClientCommandFromServer("css_lr");
    }

    if (!shouldGrantCredits()) return;
    var eco = API.Gangs?.Services.GetService<IEcoManager>();
    if (eco == null) return;
    var survivors = Utilities.GetPlayers()
     .Where(p => p is { IsBot: false, PawnIsAlive: true })
     .Select(p => new PlayerWrapper(p))
     .ToList();
    Task.Run(async () => {
      foreach (var survivor in survivors) {
        await eco.Grant(survivor, survivor.Team == CsTeam.Terrorist ? 65 : 60,
          reason: "LR Reached");
      }
    });
  }

  public bool InitiateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type) {
    var lr = factory!.CreateLastRequest(prisoner, guard, type);
    lr.Setup();
    ActiveLRs.Add(lr);

    API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST",
      $"{prisoner.SteamID} {type.ToFriendlyString()}"));

    prisoner.SetHealth(100);
    guard.SetHealth(100);
    prisoner.SetArmor(0);
    guard.SetArmor(0);

    messages.InformLastRequest(lr).ToAllChat();
    return true;
  }

  public bool EndLastRequest(AbstractLastRequest lr, LRResult result) {
    rainbowColorizer.StopRainbow(lr.Prisoner);
    rainbowColorizer.StopRainbow(lr.Guard);
    if (result is LRResult.GUARD_WIN or LRResult.PRISONER_WIN) {
      addRoundTimeCapped(CV_LR_BONUS_TIME.Value, CV_MAX_TIME_FOR_LR.Value);
      messages.LastRequestDecided(lr, result).ToAllChat();

      var wrapper =
        new PlayerWrapper(result == LRResult.GUARD_WIN ?
          lr.Guard :
          lr.Prisoner);
      if (shouldGrantCredits()) {
        var eco = API.Gangs?.Services.GetService<IEcoManager>();
        if (eco == null) return false;
        Task.Run(async () => await eco.Grant(wrapper,
          wrapper.Team == CsTeam.CounterTerrorist ? 35 : 20, reason: "LR Win"));
      }
    }

    API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST_RESULT",
      $"{lr.Prisoner.SteamID} {result.ToString()}"));

    lr.OnEnd(result);
    ActiveLRs.Remove(lr);
    return true;
  }

  private void OnDrop(CEntityInstance entity, CEntityInstance newparent) {
    if (!entity.IsValid) return;
    if (!Tag.WEAPONS.Contains(entity.DesignerName)
      && !Tag.UTILITY.Contains(entity.DesignerName))
      return;

    var weapon = Utilities.GetEntityFromIndex<CCSWeaponBase>((int)entity.Index);
    var owner  = weapon?.PrevOwner.Get()?.OriginalController.Get();

    if (owner == null || weapon == null || !weapon.IsValid) return;

    var lr = ((ILastRequestManager)this).GetActiveLR(owner);
    if (lr == null) return;

    if (newparent.IsValid) return;

    var color = owner.Team == CsTeam.CounterTerrorist ? Color.Blue : Color.Red;
    weapon.SetColor(color);

    if (lr is not IDropListener listener) return;
    listener.OnWeaponDrop(owner, weapon);
  }

  public static bool shouldGrantCredits() {
    if (API.Gangs == null) return false;
    return Utilities.GetPlayers().Count >= CV_MIN_PLAYERS_FOR_CREDITS.Value;
  }

  private HookResult OnTakeDamage(DynamicHook hook) {
    var info       = hook.GetParam<CTakeDamageInfo>(1);
    var playerPawn = hook.GetParam<CCSPlayerPawn>(0);

    var player = playerPawn.Controller.Value?.As<CCSPlayerController>();
    if (player == null || !player.IsValid) return HookResult.Continue;

    var attackerPawn = info.Attacker;
    var attacker     = attackerPawn.Value?.As<CCSPlayerController>();

    if (attacker == null || !attacker.IsValid) return HookResult.Continue;

    return ((IDamageBlocker)this).ShouldBlockDamage(player, attacker) ?
      HookResult.Handled :
      HookResult.Continue;
  }

  [UsedImplicitly]
  [GameEventHandler(HookMode.Pre)]
  public HookResult OnTakeDamage(EventPlayerHurt ev, GameEventInfo info) {
    var player   = ev.Userid;
    var attacker = ev.Attacker;
    if (player == null || player.Pawn.Value == null) return HookResult.Continue;
    if (!ShouldBlockDamage(player, attacker)) return HookResult.Continue;
    info.DontBroadcast = false;
    ev.DmgArmor        = ev.DmgHealth = 0;
    return HookResult.Handled;
  }

  [UsedImplicitly]
  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var lr in ActiveLRs.ToList())
      EndLastRequest(lr, LRResult.TIMED_OUT);

    IsLREnabled = false;
    return HookResult.Continue;
  }

  [UsedImplicitly]
  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    IsLREnabledForRound = true;
    IsLREnabled         = false;
    foreach (var player in Utilities.GetPlayers())
      MenuManager.CloseActiveMenu(player);

    foreach (var lr in ActiveLRs.ToList())
      EndLastRequest(lr, LRResult.TIMED_OUT);
    ActiveLRs.Clear();
    return HookResult.Continue;
  }

  [UsedImplicitly]
  [GameEventHandler]
  public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal() || RoundUtil.IsWarmup())
      return HookResult.Continue;

    // Handle active LRs
    var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
    if (activeLr != null && activeLr.State != LRState.COMPLETED) {
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

  [UsedImplicitly]
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

  private void addRoundTimeCapped(int time, int max) {
    var timeleft                    = RoundUtil.GetTimeRemaining();
    if (timeleft + time > max) time = max - timeleft;
    RoundUtil.AddTimeRemaining(time);
  }
}