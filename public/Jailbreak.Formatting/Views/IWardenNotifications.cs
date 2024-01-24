using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IWardenNotifications
{
	public IView PICKING_SHORTLY { get; }
	public IView NO_WARDENS { get; }
	public IView WARDEN_LEFT { get; }
	public IView WARDEN_DIED { get; }
	public IView BECOME_NEXT_WARDEN { get; }
	public IView JOIN_RAFFLE { get; }
	public IView LEAVE_RAFFLE { get; }

	/// <summary>
	/// Create a view for when the specified player passes warden
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public IView PASS_WARDEN(CCSPlayerController player);

	/// <summary>
	/// Create a view for when this player becomes a new warden
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public IView NEW_WARDEN(CCSPlayerController player);

	/// <summary>
	/// Format a response to a request about the current warden.
	/// When player is null, instead respond stating that there is no warden.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public IView CURRENT_WARDEN(CCSPlayerController? player);
}
