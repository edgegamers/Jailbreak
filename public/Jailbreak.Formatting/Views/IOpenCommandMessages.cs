using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IOpenCommandMessages {
  public IView CellsOpened { get; }
  public IView OpeningFailed { get; }
  public IView CannotOpenYet(int seconds);
}