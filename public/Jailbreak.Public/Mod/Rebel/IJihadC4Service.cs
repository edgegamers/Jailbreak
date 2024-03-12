
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Rebel;

public interface IJihadC4Service
{

    /// <summary>
    /// Calls the GiveJihadC4ToTerrorist function on a randomly chosen Terrorist 
    /// at the start of each round, subject to certain conditions (such as if there
    /// are more than 6 terrorists at the start of the round).
    /// </summary>
    public void GiveJihadC4ToRandomTerrorist();

    /// <summary>
    /// Give the "Jihad C4" to the specified Terrorist. This bomb is special and when 
    /// the +drop key is pressed when the bomb is held it will blow up and 
    /// physically push the Terrorists as well as damage the Guards in a certain radius.
    /// This function DOES deal with managing the player's state (such as if their bomb is active etc).
    /// </summary>
    /// <param name="terrorist"></param>
    public void GiveJihadC4ToTerrorist(CCSPlayerController terrorist);

    /// <summary>
    /// The function that should actually do all the calculations and placements for detonating the 
    /// "Jihad C4". This function does no validation of checking if the supplied player controller is even a Terrorist,
    /// or if they have the Jihad C4 or not. This function will create an explosion at the player's absolute origin.
    /// </summary>
    /// <param name="terrorist"></param>
    public void TryBlowUpJihadC4(CCSPlayerController terrorist);

    /// <summary>
    /// Attempts to defuse the "Jihad C4" by disabling the Terrorist from being able to 
    /// use the bomb. This should only work if the supplied player controller actually has
    /// the "Jihad C4" and is holding it out.
    /// 
    /// The default command for this should be css_defuse
    /// </summary>
    /// <param name="terrorist"></param>
    public void TryDefuseJihadC4(CCSPlayerController terrorist);

    /// <summary>
    /// Attempts to set a delay on the "Jihad C4". This means when the bomb is successfully 
    /// used it should not immediately explode (as is the default) but instead explode after the 
    /// given delay. 
    /// </summary>
    /// <param name="terrorist"></param>
    public void TrySetDelayJihadC4(CCSPlayerController terrorist, float delay);

}
