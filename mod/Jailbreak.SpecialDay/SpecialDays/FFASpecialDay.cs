using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FFASpecialDay : AbstractSpecialDay {
  public FFASpecialDay(BasePlugin plugin) : base(plugin) { }
  public override SDType Type { get; }
  public override ISpecialDayMessages Messages => new FFAMessages();
  public override void Setup() { throw new NotImplementedException(); }
  public override void Execute() { throw new NotImplementedException(); }

  public override HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    throw new NotImplementedException();
  }
}