using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay;

public class SpecialDayManager(ISpecialDayFactory factory)
  : ISpecialDayManager {
  public bool IsSDEnabled { get; set; } = false;
  public SDType? ActiveSD { get; private set; } = null;

  private AbstractSpecialDay? activeDay;

  public bool InitiateSpecialDay(SDType type) {
    activeDay = factory.CreateSpecialDay(type);
    activeDay.Messages.SpecialDayStart.ToAllChat();
    
    activeDay.Setup();
    return true;
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    if (!IsSDEnabled || activeDay == null) return HookResult.Continue;
    IsSDEnabled = false;
    ActiveSD    = null;

    activeDay.Messages.SpecialDayEnd((CsTeam)@event.Winner).ToAllChat();
    activeDay = null;
    return HookResult.Continue;
  }
}