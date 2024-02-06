using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Logs;

public class LogEntityListeners : IPluginBehavior
{
	private readonly IRichLogService _logs;

	public LogEntityListeners(IRichLogService logs)
	{
		_logs = logs;
	}

	[EntityOutputHook("func_button", "OnPressed")]
	public HookResult OnButtonPressed(CEntityIOOutput output, string name, CEntityInstance activator,
		CEntityInstance caller, CVariant value, float delay)
	{
		if (!activator.IsValid)
			return HookResult.Continue;
		int index = (int)activator.Index;
		CCSPlayerPawn? pawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(index);
		if (!pawn.IsValid)
			return HookResult.Continue;
		if (!pawn.OriginalController.IsValid)
			return HookResult.Continue;
		CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);
		if (!ent.IsValid)
			return HookResult.Continue;
		_logs.Append(
			$"{_logs.Player(pawn.OriginalController.Value!)} pressed a button {ent.Entity?.Name ?? "Unlabeled"} -> {output?.Connections?.TargetDesc ?? "None"}");
		return HookResult.Continue;
	}
}
