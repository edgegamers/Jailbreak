using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayInstanceMessages {
  public string Name { get; }
  public string? Description { get; }

  public IView SpecialDayStart { get; }

  public IView SpecialDayEnd() {
    return new SimpleView { ISpecialDayMessages.PREFIX, Name, "ended." };
  }

  public IView BeginsIn(int seconds);
  //   return seconds == 0 ?
  //     new SimpleView { ISpecialDayMessages.PREFIX, Name, "begins now!" } :
  //     new SimpleView {
  //       ISpecialDayMessages.PREFIX,
  //       Name,
  //       "begins in",
  //       seconds,
  //       "seconds."
  //     };
  // }
}