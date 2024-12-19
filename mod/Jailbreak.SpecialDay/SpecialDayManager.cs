using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.SpecialDayColorPerk;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.SpecialDay.SpecialDays;
using Microsoft.Extensions.DependencyInjection;
using MStatsShared;

namespace Jailbreak.SpecialDay;

public class SpecialDayManager(ISpecialDayFactory factory)
  : ISpecialDayManager {
  public bool IsSDRunning { get; set; }
  public AbstractSpecialDay? CurrentSD { get; private set; }
  public int RoundsSinceLastSD { get; set; }

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
    var gangCache = new Dictionary<int, (char, Color)?>();
    foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot)) {
      var wrapper = new PlayerWrapper(player);
      Task.Run(async () => {
        var gangPlayer = await players.GetPlayer(wrapper.Steam);
        if (gangPlayer?.GangId == null) return;
        var gangId = gangPlayer.GangId.Value;
        if (!gangCache.TryGetValue(gangId, out var color)) {
          var (success, data) =
            await gangStats.GetForGang<SDColorData>(gangId,
              SDColorPerk.STAT_ID);
          if (!success || data == null) {
            gangCache[gangId] = null;
            return;
          }

          var col = data.Equipped.GetColor() ?? data.Unlocked.PickRandom();

          if (col == null) {
            gangCache[gangId] = null;
            return;
          }

          color             = (col.Value.GetChatColor(), col.Value);
          gangCache[gangId] = color;
        }

        if (color != null) {
          await Server.NextFrameAsync(() => player.SetColor(color.Value.Item2));
          wrapper.PrintToChat(
            $" {ChatColors.DarkBlue}Gangs> {ChatColors.Grey}Your gang will be {color.Value.Item1}{color.Value.Item2.Name}{ChatColors.Grey} this special day.");
        }
      });
    }
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