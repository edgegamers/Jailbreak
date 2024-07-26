using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Handles announcing admin actions to the entire server.
/// </summary>
public interface IAnnouncer
{
    /// <summary>
    ///     Announces that an admin has performed an action.
    ///     Implementation may decide to mask / anonymize the admin name.
    ///     Callers should not withold information- this should be done by the implementation.
    ///     Example:
    ///     Announce("MSWS", "Skle", "gagged") -> "MSWS gagged Skle"
    ///     Announce("MSWS", "Skle", "gagged", " for 1 hour") -> "MSWS gagged Skle for 1 hour"
    /// </summary>
    /// <param name="admin">The name of admin performing the action</param>
    /// <param name="target">The target of the action</param>
    /// <param name="action">The action (likely verb)</param>
    /// <param name="suffix">A suffix message to append to the announcement (or empty)</param>
    void Announce(string admin, string target, string action, string suffix = "");

    void Announce(CCSPlayerController? admin, string target, string action, string suffix = "")
    {
        Announce(admin == null ? "Console" : admin.PlayerName, target, action, suffix);
    }
}