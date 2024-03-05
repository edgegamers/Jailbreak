
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenPeaceService
{

    /// <summary>
    /// Self expanatory. Uses the API exposed in the WardenBehavior class.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool IsWarden(CCSPlayerController? player);
    // todo document saying that by default all admins SHOULD bypass this mute (not implemented yet)

    /// <summary>
    /// This function is responsible for muting teams of players. 
    /// It uses the PeaceMuteOptions class to "configure" the various options such as teams to target and duration of mute.
    /// Admins should be able to bypass this (TODO).
    /// </summary>
    /// <param name="options"></param>
    public void PeaceMute(PeaceMuteOptions options);

    /// <summary>
    /// Returns if any peace-mute is active (prisoner mute round start, css_peace, or first warden peace-mute).
    /// </summary>
    /// <returns></returns>
    public bool GetPeaceMuteActive();

    /// <summary>
    /// Unmutes the teams of the players specified, respecting was already muted before any peace-mute was initiated, and sends a message to all players indicating the unmute.
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="target"></param>
    public void UnmutePrevMutedPlayers(PeaceMuteOptions.MuteReason reason, params CsTeam[] target);

}
