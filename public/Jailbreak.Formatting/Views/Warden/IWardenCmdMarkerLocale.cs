using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenCmdMarkerLocale {

  public IView ChangingNotEnabled { get; }
  IView TypeChanged(string type);
  IView ColorChanged(string color);
}