using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.SpecialDay;

public class SoloDayMessages(string name, string? description = null)
  : ISpecialDayInstanceMessages {
  public string Name => name;
  public string? Description => description;

  public IView SpecialDayStart
    => Description == null ?
      new SimpleView { ISpecialDayMessages.PREFIX, Name, "has begun!" } :
      new SimpleView {
        ISpecialDayMessages.PREFIX,
        Name,
        "has begun!",
        SimpleView.NEWLINE,
        ISpecialDayMessages.PREFIX,
        Description
      };

  public IView SpecialDayEnd() {
    var lastAlive = PlayerUtil.GetAlive().FirstOrDefault();
    if (lastAlive == null)
      return new SimpleView {
        ISpecialDayMessages.PREFIX, Name, "has ended! No one won!"
      };
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      Name,
      "ended!",
      lastAlive,
      "won!"
    };
  }
}