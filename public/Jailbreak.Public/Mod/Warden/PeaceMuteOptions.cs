using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Warden;

public class PeaceMuteOptions
{

    public enum MuteReason
    {
        CSS_PEACE,
        FIRSTWARDEN,
        PRISONERS_STARTROUND,
        END_ROUND,
        WARDEN_DIED,
        ADMIN_REMOVED_PEACEMUTE
    }

    private readonly MuteReason reason;
    private readonly CsTeam[] teams;
    private readonly float timeSeconds;

    /// <summary>
    /// A class to specify the MuteReason and targets (teams) that should be affected by the IWardenPeaceService#PeaceMute function.
    /// This object should be passed to the PeaceMute function.
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="timeSeconds"></param>
    /// <param name="teams"></param>
    public PeaceMuteOptions(MuteReason reason, float timeSeconds = 0, params CsTeam[] teams)
    {
        this.reason = reason;
        this.teams = teams;
        this.timeSeconds = timeSeconds;
    }

    public MuteReason GetReason() { return reason; }
    public CsTeam[] GetTargetTeams() { return teams; }
    public float GetTimeSeconds() { return timeSeconds; }   

}
