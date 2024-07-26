using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Responsible for tracking and managing heat.
///     Heat is a measure of how much a player is spamming, with 0 being no heat and 1 being maximum heat.
///     It is recommended to call IssueHeatWarning when a player reaches 0.75 heat.
/// </summary>
public interface IHeatManager
{
    /// <summary>
    ///     Resets the heat of all players.
    /// </summary>
    void ResetHeat();

    /// <summary>
    ///     Resets the heat of a specific player.
    /// </summary>
    /// <param name="steamId">SteamID to reset</param>
    void ResetHeat(ulong steamId);

    /// <summary>
    ///     Bumps the heat of a specific player.
    /// </summary>
    /// <param name="steamId">SteamID to bump</param>
    /// <param name="amount">Amount of heat to bump</param>
    /// <returns></returns>
    float BumpHeat(ulong steamId, float amount = 0.15f);

    /// <summary>
    ///     Returns an estimated amount of heat that a message should add.
    ///     Generally, short messages are discouraged, medium messages are neutral, and long messages are discouraged.
    /// </summary>
    /// <param name="message">Message to get heat of</param>
    /// <returns>A float representing the message's heat</returns>
    float GetMessageHeat(string message);

    /// <summary>
    ///     Returns the current heat of a player.
    /// </summary>
    /// <param name="steamId">SteamID to check</param>
    /// <returns>The heat of the player, or 0 if none</returns>
    float GetCurrentHeat(ulong steamId);

    /// <summary>
    ///     Issues a heat warning to a player.
    /// </summary>
    /// <param name="player">Player to issue warning to</param>
    void IssueHeatWarning(CCSPlayerController player);
}