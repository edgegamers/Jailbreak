using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay;

public class SpecialDayManager : ISpecialDayManager {
  public bool IsSDEnabled { get; set; } = false;
  public SDType? ActiveSD { get; private set; } = null;

  public bool InitiateSpecialDay(SDType type) {
    throw new NotImplementedException();
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    if (!IsSDEnabled) return HookResult.Continue;
    IsSDEnabled = false;
    ActiveSD    = null;

    return HookResult.Continue;
  }
}