using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Responsible for gagging players.
/// </summary>
public interface IGagService
{
    /// <summary>
    ///     Gags a player.
    /// </summary>
    /// <param name="sender">Player to gag</param>
    /// <returns>True if the player was gagged, false if the player was already gagged (or is an invalid player)</returns>
    bool GagPlayer(CCSPlayerController sender)
    {
        return GagPlayer(sender.SteamID);
    }

    bool GagPlayer(ulong steamId);

    /// <summary>
    ///     Ungags a player.
    /// </summary>
    /// <param name="sender">Player to ungag</param>
    /// <returns>True if the player was ungagged, false if the player was not originally gagged (or is an invalid player)</returns>
    bool UngagPlayer(CCSPlayerController sender)
    {
        return UngagPlayer(sender.SteamID);
    }

    bool UngagPlayer(ulong steamId);

    /// <summary>
    ///     Checks if a player is gagged.
    /// </summary>
    /// <param name="sender">Player to check</param>
    /// <returns>True if the player is gagged, false if the player is not (or is an invalid player)</returns>
    bool IsGagged(CCSPlayerController sender)
    {
        return IsGagged(sender.SteamID);
    }

    bool IsGagged(ulong steamId);
}