using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayInstanceMessages {
  public string Name { get; }
  public string? Description { get; }

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

  public IView SpecialDayEnd()
    => new SimpleView { ISpecialDayMessages.PREFIX, Name, "ended." };

  public IView BeginsIn(int seconds)
    => new SimpleView {
      ISpecialDayMessages.PREFIX,
      Name,
      "begins in",
      seconds,
      "seconds."
    };
}