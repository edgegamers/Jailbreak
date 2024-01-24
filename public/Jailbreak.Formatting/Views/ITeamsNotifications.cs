using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IRatioNotifications
{
	public IView NOT_ENOUGH_GUARDS { get; }

	public IView PLEASE_JOIN_GUARD_QUEUE { get; }

	public IView JOINED_GUARD_QUEUE { get; }

	public IView ALREADY_A_GUARD { get; }


	public IView YOU_WERE_AUTOBALANCED_PRISONER { get; }

	public IView YOU_WERE_AUTOBALANCED_GUARD { get; }

	public IView ATTEMPT_TO_JOIN_FROM_TEAM_MENU { get; }

	public IView LEFT_GUARD { get; }

}
