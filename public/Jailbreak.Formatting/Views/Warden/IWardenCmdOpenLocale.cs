using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenCmdOpenLocale {
  public IView CellsOpened { get; }
  public IView OpeningFailed { get; }
  public IView AlreadyOpened { get; }
  public IView CannotOpenYet(int seconds);
}