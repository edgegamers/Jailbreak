using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Warden.Commands;

public class SpecialTreatmentCommandsBehavior
{

	[ConsoleCommand("css_treat", "Grant or revoke special treatment from a player")]
	[ConsoleCommand("css_st", "Grant or revoke special treatment from a player")]
	[CommandHelper(0, "css_st / css_treat <player>", CommandUsage.CLIENT_ONLY)]
	public void Command_Toggle(CCSPlayerController? player, CommandInfo command)
	{

	}
}
