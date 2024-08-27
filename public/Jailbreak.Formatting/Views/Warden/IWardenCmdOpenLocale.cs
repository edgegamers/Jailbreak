using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenCmdOpenLocale {
  /// <summary>
  ///   The cells were auto-opened.
  /// </summary>
  public IView CellsOpened { get; }

  public IView OpeningFailed { get; }
  public IView AlreadyOpened { get; }

  /// <summary>
  ///   The cells were opened by the specified player.
  ///   If the player is null, the cells were opened by the warden.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public IView CellsOpenedBy(CCSPlayerController? player);

  public IView CannotOpenYet(int seconds);
}