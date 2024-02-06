namespace Jailbreak.Teams.Queue;

public class QueueState
{
	/// <summary>
	///     The counter value when the player entered the queue
	///     Lower = join CT sooner
	/// </summary>
	public int Position { get; set; }

	/// <summary>
	///     True when this player is currently in the queue
	/// </summary>
	public bool InQueue { get; set; }

	/// <summary>
	///     This player is allowed to be on the CT team.
	///     If this is false, they will be swapped back to prisoner.
	/// </summary>
	public bool IsGuard { get; set; }
}