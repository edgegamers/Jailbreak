using CounterStrikeSharp.API.Core;

namespace Jailbreak.Generic.PlayerState;

public interface ITrackedPlayerState
{
    /// <summary>
    ///     Reset a state for a specific player
    /// </summary>
    /// <param name="controller"></param>
    void Reset(CCSPlayerController controller);

    /// <summary>
    ///     Reset states for all players
    /// </summary>
    void Drop();
}