using Jailbreak.Formatting.Base;
using Jailbreak.Public.Utils;

namespace Jailbreak.Formatting.Views;

public interface IOpenCommandMessages {
  public IView OpenedCells { get; }
  public IView OpenResult(Sensitivity sensitivity);
}