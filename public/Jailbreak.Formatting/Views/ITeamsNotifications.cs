using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IRatioNotifications {
  public IView NotEnoughGuards { get; }

  public IView PleaseJoinGuardQueue { get; }

  public IView JoinedGuardQueue { get; }

  public IView AlreadyAGuard { get; }


  public IView YouWereAutobalancedPrisoner { get; }

  public IView YouWereAutobalancedGuard { get; }

  public IView AttemptToJoinFromTeamMenu { get; }

  public IView LeftGuard { get; }
}