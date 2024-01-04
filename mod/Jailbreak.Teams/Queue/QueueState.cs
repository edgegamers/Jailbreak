namespace Jailbreak.Teams.Queue;

public class QueueState
{

	public QueueState()
	{
	}

	/// <summary>
	/// The counter value when the player entered the queue
	/// Lower = join CT sooner
	/// </summary>
	public int Position { get; set; }

	public bool InQueue { get; set; }

	public bool IsGuard { get; set; }
}
