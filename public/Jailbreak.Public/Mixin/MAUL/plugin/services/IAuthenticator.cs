using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Handles authenticating players and granting permissions.
///     Each IAuthenticator should automatically verify players on join.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    ///     Verifies a player and returns true if verification was successful.
    ///     Unsuccessful verification may be due to a bot player, MAUL being down, etc.
    ///     Due to the async nature of this method, use PlayerInfo(CCSPlayerController) to create a new PlayerInfo object.
    /// </summary>
    /// <param name="player">The player to verify</param>
    /// <param name="info">The player's info</param>
    /// <returns>True if the player was verified</returns>
    public Task<bool> VerifyPlayer(CCSPlayerController player, PlayerInfo info, bool joined = false);
}