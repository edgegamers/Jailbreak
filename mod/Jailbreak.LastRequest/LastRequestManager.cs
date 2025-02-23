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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MStatsShared;

namespace Jailbreak.LastRequest;

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

  public bool ShouldBlockDamage(CCSPlayerController player,
    CCSPlayerController? attacker) {
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
    VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnCanAcquire,
      HookMode.Pre);
  }

  public void Dispose() {
    VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnCanAcquire,
      HookMode.Pre);

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

    API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST_ACTIVATED"));

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
        await incrementLRReached(survivor);
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

    var prisonerWrapper = new PlayerWrapper(prisoner);
    var guardWrapper    = new PlayerWrapper(guard);

    Task.Run(async () => {
      await incrementLRStart(prisonerWrapper);
      await incrementLRStart(guardWrapper);

      await colorForLR(prisonerWrapper, guardWrapper);
    });

    messages.InformLastRequest(lr).ToAllChat();
    return true;
  }

  public bool EndLastRequest(AbstractLastRequest lr, LRResult result) {
    rainbowColorizer.StopRainbow(lr.Prisoner);
    rainbowColorizer.StopRainbow(lr.Guard);
    if (result is LRResult.GUARD_WIN or LRResult.PRISONER_WIN) {
      // RoundUtil.AddTimeRemaining(CV_LR_BONUS_TIME.Value);
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

      if (API.Gangs != null)
        Task.Run(async () => await incrementLRWin(wrapper));
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

  private HookResult OnCanAcquire(DynamicHook hook) {
    if (ActiveLRs.Count == 0) return HookResult.Continue;
    var player = hook.GetParam<CCSPlayer_ItemServices>(0)
     .Pawn.Value.Controller.Value?.As<CCSPlayerController>();
    var data = VirtualFunctions.GetCSWeaponDataFromKey.Invoke(-1,
      hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString());

    if (player == null || !player.IsValid) return HookResult.Continue;

    var method = hook.GetParam<AcquireMethod>(2);
    if (method != AcquireMethod.PickUp) return HookResult.Continue;

    if (ActiveLRs.Any(lr => lr.PreventEquip(player, data))) {
      hook.SetReturn(AcquireResult.NotAllowedByMode);
      return HookResult.Handled;
    }

    return HookResult.Continue;
  }

  private async Task colorForLR(PlayerWrapper a, PlayerWrapper b) {
    var playerStats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    var gangStats   = API.Gangs?.Services.GetService<IGangStatManager>();
    var gangs       = API.Gangs?.Services.GetService<IGangManager>();
    var localizer   = API.Gangs?.Services.GetService<IStringLocalizer>();
    if (playerStats == null || localizer == null || gangs == null
      || gangStats == null)
      return;
    var aData = await playerStats.GetForPlayer<LRColor>(a, LRColorPerk.STAT_ID);
    var bData = await playerStats.GetForPlayer<LRColor>(b, LRColorPerk.STAT_ID);

    LRColor?       toApply = null;
    PlayerWrapper? higher  = null;
    higher = await getHigherPlayer(a, b);
    if (toApply == null) return;
    if (a.Player == null || b.Player == null) return;

    var higherGang = await gangs.GetGang(higher.Steam);
    if (higherGang == null) return;

    var gData =
      await gangStats.GetForGang<LRColor>(higherGang, LRColorPerk.STAT_ID);

    if ((gData & toApply.Value) == 0) return;

    var color = toApply.Value.GetColor();

    if (color == null) { // Player picked random, but we need to pick
      // the random from their GANG's colors
      var gangData =
        await playerStats.GetForPlayer<LRColor>(higher, LRColorPerk.STAT_ID);
      color = gangData.PickRandomColor();
    }

    if (color == null) return;

    await Server.NextFrameAsync(() => {
      if (toApply == LRColor.RAINBOW) {
        rainbowColorizer.StartRainbow(a.Player);
        rainbowColorizer.StartRainbow(b.Player);
        var rmsg = localizer.Get(MSG.PREFIX)
          + $"Your LR will be {IRainbowColorizer.RAINBOW}.";

        a.Player.PrintToChat(rmsg);
        b.Player.PrintToChat(rmsg);
        return;
      }

      a.Player.SetColor(color.Value);
      b.Player.SetColor(color.Value);

      var msg = localizer.Get(MSG.PREFIX)
        + $"Your LR will be {color.GetChatColor()}{color.Value.Name}{ChatColors.Grey}.";

      a.Player.PrintToChat(msg);
      b.Player.PrintToChat(msg);
    });
  }

  private async Task<PlayerWrapper> getHigherPlayer(PlayerWrapper a,
    PlayerWrapper b) {
    var leaderboard = API.Gangs?.Services.GetService<ILeaderboard>();
    var players     = API.Gangs?.Services.GetService<IPlayerManager>();
    if (leaderboard == null || players == null) return a;
    var aGangPlayer = await players.GetPlayer(a.Steam);
    var bGangPlayer = await players.GetPlayer(b.Steam);

    if (aGangPlayer == null && bGangPlayer != null) return b;
    if (aGangPlayer != null && bGangPlayer == null) return a;

    if (aGangPlayer == null || bGangPlayer == null) return a;

    var aGang = aGangPlayer.GangId;
    var bGang = bGangPlayer.GangId;

    if (aGang == null && bGang != null) return b;
    if (aGang != null && bGang == null) return a;

    if (aGang == null || bGang == null) return a;
    if (aGang == bGang) return a;

    var aRank = await leaderboard.GetPosition(aGang.Value);
    var bRank = await leaderboard.GetPosition(bGang.Value);

    if (aRank == null && bRank != null) return b;
    if (aRank != null && bRank == null) return a;

    if (aRank == null || bRank == null) return a;

    return aRank < bRank ? a : b;
  }

  private async Task incrementLRReached(PlayerWrapper player) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return;
    var stat = await getStat(player);
    if (stat == null) return;

    if (player.Team == CsTeam.Terrorist)
      stat.LRsReachedAsT++;
    else
      stat.LRsReachedAsCt++;

    await stats.SetForPlayer(player, LRStat.STAT_ID, stat);
  }

  private async Task incrementLRStart(PlayerWrapper player) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return;
    var stat = await getStat(player);
    if (stat == null) return;

    if (player.Team == CsTeam.Terrorist)
      stat.TLrs++;
    else
      stat.CtLrs++;

    await stats.SetForPlayer(player, LRStat.STAT_ID, stat);
  }

  private async Task incrementLRWin(PlayerWrapper player) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return;
    var stat = await getStat(player);
    if (stat == null) return;

    if (player.Team == CsTeam.Terrorist)
      stat.TLrsWon++;
    else
      stat.CTLrsWon++;

    await stats.SetForPlayer(player, LRStat.STAT_ID, stat);
  }

  private async Task<LRData?> getStat(PlayerWrapper player) {
    var stats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    if (stats == null) return null;
    var data = await stats.GetForPlayer<LRData>(player, LRStat.STAT_ID)
      ?? new LRData();
    return data;
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

  [GameEventHandler]
  public HookResult OnTakeDamage(EventPlayerHurt ev, GameEventInfo info) {
    var player   = ev.Userid;
    var attacker = ev.Attacker;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    if (!ShouldBlockDamage(player, attacker)) return HookResult.Continue;
    if (player.PlayerPawn.IsValid) {
      var playerPawn = player.PlayerPawn.Value!;
      playerPawn.Health = playerPawn.LastHealth;
    }

    info.DontBroadcast = false;
    ev.DmgArmor        = ev.DmgHealth = 0;
    return HookResult.Handled;
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var lr in ActiveLRs.ToList())
      EndLastRequest(lr, LRResult.TIMED_OUT);

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