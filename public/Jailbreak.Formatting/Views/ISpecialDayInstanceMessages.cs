using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayInstanceMessages {
  public string Name { get; }
  public IView SpecialDayStart { get; }
  public IView SpecialDayEnd(CsTeam winner);
}