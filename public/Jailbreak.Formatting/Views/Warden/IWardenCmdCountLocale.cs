using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenCmdCountLocale {
  public IView PrisonersInMarker(int prisoners);
  public IView CannotCountYet(int seconds);
  public IView NoMarkerSet { get; }
}