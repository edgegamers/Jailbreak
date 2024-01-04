namespace Jailbreak.Warden.Queue;

public class QueueFavorState
{

	public const int BASE_TICKETS = 2;

	public int RoundsWithoutWarden { get; set; } = 0;

	public int Favor { get; set; } = 0;

	public int GetTickets()
	{
		return BASE_TICKETS
		       + Favor
		       + RoundsWithoutWarden;
	}

}
