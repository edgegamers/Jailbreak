using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Rebel;

public interface IJihadC4Service
{

    /// <summary>
    /// Tries to give the jihad C4 to the player, assuming the player object passed in is valid.
    /// </summary>
    /// <param name="player"></param>
    void TryGiveC4ToPlayer(CCSPlayerController player);

    /// <summary>
    /// Tries to detonate the jihad c4, relative to the player's position after the delay.
    /// </summary>
    /// <param name="player"></param>
    void TryDetonateJihadC4(CCSPlayerController player, float delay);

}
