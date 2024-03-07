using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenLastGuardService
{

    public void TryActivateLastGuard();

    public void TryDeactivateLastGuard();

    public bool LastGuardPossible();

    public delegate void GuardCallback(CCSPlayerController player);

    /// <summary>
    /// Loops through all valid players on the server and invokes the callback only
    /// if the current player being looped is on the CounterTerrorist team.
    /// </summary>
    /// <param name="callback"></param>
    public void IterateThroughTeam(GuardCallback callback, CsTeam team);

}
