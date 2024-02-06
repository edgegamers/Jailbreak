namespace Jailbreak.Warden.Selection;

public class QueueFavorState
{
    public const int BaseTickets = 2;

    public int RoundsWithoutWarden { get; set; } = 0;

    public int Favor { get; set; } = 0;

    public int GetTickets()
    {
        return BaseTickets
               + Favor
               + RoundsWithoutWarden;
    }
}