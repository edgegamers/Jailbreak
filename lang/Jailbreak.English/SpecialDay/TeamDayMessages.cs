using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.SpecialDay;

public class TeamDayMessages(string name, string? description = null)
  : ISpecialDayInstanceMessages {
  public string Name => name;
  public string? Description => description;

  public virtual IView SpecialDayStart
    => ((ISpecialDayInstanceMessages)this).SpecialDayStart;

  public virtual IView BeginsIn(int s)
    => ((ISpecialDayInstanceMessages)this).BeginsIn(s);

  public IView SpecialDayEnd() {
    var winner = PlayerUtil.GetAlive().FirstOrDefault()?.Team
      ?? CsTeam.Spectator;
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      Name,
      "ended.",
      (winner == CsTeam.CounterTerrorist ? ChatColors.Blue : ChatColors.Red)
      + (winner == CsTeam.CounterTerrorist ? "CTs" : "Ts"),
      "won!"
    };
  }
}