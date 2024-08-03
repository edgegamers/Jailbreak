using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.SpecialDay.SpecialDays;
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

    CurrentSD.Setup();
    return true;
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