using Jailbreak.Formatting.Base;
using Jailbreak.Public.Utils;

namespace Jailbreak.Formatting.Views;

public interface IOpenCommandMessages {
  public IView CannotOpenYet(int seconds);
  public IView OpenResult(Sensitivity? sensitivity);
}