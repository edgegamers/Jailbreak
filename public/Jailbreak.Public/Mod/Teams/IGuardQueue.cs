using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Teams;

public interface IGuardQueue
{
	/// <summary>
	///     Get all players in the queue
	/// </summary>
	IEnumerable<CCSPlayerController> Queue { get; }


    bool TryEnterQueue(CCSPlayerController player);

    bool TryExitQueue(CCSPlayerController player);

    /// <summary>
    ///     Pop the provided amount of players from the queue
    ///     and put them on the CT team
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    bool TryPop(int count);

    /// <summary>
    ///     Pull the provided amount of players from the CT team
    ///     and put them back in the queue.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    bool TryPush(int count);

    /// <summary>
    ///     Force the player to join CT, without being pushed back to T.
    /// </summary>
    /// <param name="player"></param>
    void ForceGuard(CCSPlayerController player);

    /// <summary>
    ///     Get this player's position in the queue
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    int GetQueuePosition(CCSPlayerController player);
}