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

    // if no timeSeconds is set then no muting is gonna happen
    // todo do this instead: PeaceMuteOptions(reason, timeSeconds, params CsTeam[])
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
