﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl.Extensions;
using Gangs.SpecialDayColorPerk;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rainbow;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Utils;
using Jailbreak.SpecialDay.SpecialDays;
using Microsoft.Extensions.DependencyInjection;
using MStatsShared;

namespace Jailbreak.SpecialDay;

public class SpecialDayManager(ISpecialDayFactory factory,
  IServiceProvider provider, IWardenLocale wardenMsg, ISDLocale locale)
  : ISpecialDayManager {
  public static readonly FakeConVar<int> CV_MAX_ELAPSED_TIME = new(
    "css_jb_sd_max_elapsed_time",
    "Max time elapsed in a round to be able to call a special day", 30);

  public static readonly FakeConVar<int> CV_ROUNDS_BETWEEN_SD = new(
    "css_jb_sd_round_cooldown", "Rounds between special days", 5);

  private readonly IRainbowColorizer colorizer =
    provider.GetRequiredService<IRainbowColorizer>();

  public bool IsSDRunning { get; set; }
  public AbstractSpecialDay? CurrentSD { get; private set; }
  public int RoundsSinceLastSD { get; set; }

  public bool CanStartSpecialDay(SDType type, CCSPlayerController? player,
    bool print = true) {
    var warden = provider.GetRequiredService<IWardenService>();
    if (!AdminManager.PlayerHasPermissions(player, "@css/rcon")) {
      if (!warden.IsWarden(player) || RoundUtil.IsWarmup()) {
        if (print) wardenMsg.NotWarden.ToChat(player);
        return false;
      }

      if (IsSDRunning) {
        if (CurrentSD is ISpecialDayMessageProvider messaged) {
          if (print)
            locale.SpecialDayRunning(messaged.Locale.Name).ToChat(player);
          return false;
        }

        if (print)
          locale.SpecialDayRunning(CurrentSD?.Type.ToString() ?? "Unknown")
           .ToChat(player);
        return false;
      }

      var roundsToNext = RoundsSinceLastSD - CV_ROUNDS_BETWEEN_SD.Value;
      if (roundsToNext < 0) {
        if (print)
          locale.SpecialDayCooldown(Math.Abs(roundsToNext)).ToChat(player);
        return false;
      }

      if (RoundUtil.GetTimeElapsed() > CV_MAX_ELAPSED_TIME.Value) {
        if (print)
          locale.TooLateForSpecialDay(CV_MAX_ELAPSED_TIME.Value).ToChat(player);
        return false;
      }
    }

    var denyReason = type.CanCall(player);

    if (denyReason == null
      || AdminManager.PlayerHasPermissions(player, "@css/root"))
      return true;
    if (print) locale.CannotCallDay(denyReason).ToChat(player);
    return false;
  }

  public bool InitiateSpecialDay(SDType type) {
    API.Stats?.PushStat(new ServerStat("JB_SPECIALDAY", type.ToString()));
    RoundsSinceLastSD = 0;
    CurrentSD         = factory.CreateSpecialDay(type);
    IsSDRunning       = true;
    if (CurrentSD is ISpecialDayMessageProvider messaged)
      messaged.Locale.SpecialDayStart.ToAllChat();

    assignGangColors();
    CurrentSD.Setup();
    return true;
  }

  private void assignGangColors() {
    if (API.Gangs == null) return;
    var players   = API.Gangs.Services.GetService<IPlayerManager>();
    var gangStats = API.Gangs.Services.GetService<IGangStatManager>();
    if (players == null || gangStats == null) return;
    var playerWrappers = Utilities.GetPlayers()
     .Where(p => !p.IsBot)
     .Select(p => new PlayerWrapper(p))
     .ToList();
    Task.Run(async () => {
      foreach (var wrapper in playerWrappers) {
        var gangPlayer = await players.GetPlayer(wrapper.Steam);
        if (gangPlayer?.GangId == null) return;
        var gangId = gangPlayer.GangId.Value;
        var data =
          await gangStats.GetForGang<SDColorData>(gangId, SDColorPerk.STAT_ID);
        if (data == null) continue;

        var col = data.Equipped.GetColor() ?? data.Unlocked.PickRandom();

        if (col == null) continue;

        if (data.Equipped == SDColor.RAINBOW) {
          if (wrapper.Player != null)
            await Server.NextFrameAsync(()
              => colorizer.StartRainbow(wrapper.Player));
          wrapper.PrintToChat(
            $" {ChatColors.DarkBlue}Gangs> {ChatColors.Grey}Your gang will be {IRainbowColorizer.RAINBOW} this special day.");
          continue;
        }

        if (wrapper.Player != null)
          await Server.NextFrameAsync(() => wrapper.Player.SetColor(col.Value));
        wrapper.PrintToChat(
          $" {ChatColors.DarkBlue}Gangs> {ChatColors.Grey}Your gang will be {col.GetChatColor()}{col.Value.Name}{ChatColors.Grey} this special day.");
      }
    });
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    if (RoundUtil.IsWarmup()) return HookResult.Continue;
    RoundsSinceLastSD++;
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    if (!IsSDRunning || CurrentSD == null) return HookResult.Continue;
    IsSDRunning = false;
    if (CurrentSD is ISpecialDayMessageProvider messaged)
      messaged.Locale.SpecialDayEnd.ToAllChat();
    CurrentSD = null;
    return HookResult.Continue;
  }
}