using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class SpecialTreatmentCommandsBehavior : IPluginBehavior
{

	private IWardenService _warden;
	private ISpecialTreatmentService _specialTreatment;

	private IGenericCommandNotifications _generic;
	private ISpecialTreatmentNotifications _notifications;

	public SpecialTreatmentCommandsBehavior(IWardenService warden, ISpecialTreatmentService specialTreatment, IGenericCommandNotifications generic, ISpecialTreatmentNotifications notifications)
	{
		_warden = warden;
		_specialTreatment = specialTreatment;
		_generic = generic;
		_notifications = notifications;
	}

	[ConsoleCommand("css_treat", "Grant or revoke special treatment from a player")]
	[ConsoleCommand("css_st", "Grant or revoke special treatment from a player")]
	[CommandHelper(1, "[target]", CommandUsage.CLIENT_ONLY)]
	public void Command_Toggle(CCSPlayerController? player, CommandInfo command)
	{
		if (player == null)
			return;

		if (!_warden.IsWarden(player))
			//	You're not that warden, blud
			return;

		//	Since we have min_args, don't need to check for validity here.
		//	just only get targets that are T's.
		var targets = command.GetArgTargetResult(1);
		var eligible = targets
			.Where(player => player.Team == CsTeam.Terrorist)
			.ToList();

		if (eligible.Count == 0)
		{
			_generic.PlayerNotFound(command.GetArg(1))
				.ToPlayerChat(player)
				.ToPlayerConsole(player);
			return;
		}
		else if (eligible.Count != 1)
		{
			_generic.PlayerFoundMultiple(command.GetArg(1))
				.ToPlayerChat(player)
				.ToPlayerConsole(player);
			return;
		}

		//	One target, mark as ST.
		var special = eligible.First();

		if (_specialTreatment.IsSpecialTreatment(special))
		{
			//	Revoke
			_specialTreatment.Revoke(special);
		}
		else
		{
			//	Player does not have ST, grant
			_specialTreatment.Grant(special);
		}
	}
}
