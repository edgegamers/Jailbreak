using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs.Listeners;

public class LogEntityListeners : IPluginBehavior
{
	private readonly IRichLogService _logs;

	public LogEntityListeners(IRichLogService logs)
	{
		_logs = logs;
		OnDropGun();
	}
	
	private static MemoryFunctionVoid<CBasePlayerPawn, CBasePlayerWeapon, IntPtr> WeaponDrop = new( @"\x55\x48\x89\xE5\x41\x56\x41\x55\x49\x89\xD5\x41\x54\x49\x89\xFC\x53\x48\x89\xF3\xE8\x2A\x2A\x2A\x2A" );

	private void OnDropGun()
	{
		WeaponDrop.Hook(hook =>
		{
			var pawn = hook.GetParam<CBasePlayerPawn>(0);
			var weapon = hook.GetParam<CBasePlayerWeapon>(1);

			if (!pawn.IsValid) return HookResult.Continue;
			if (pawn.Controller.Value == null || !pawn.Controller.IsValid) return HookResult.Continue;

			var player = (CCSPlayerController)pawn.Controller.Value;

			if (player.Team != CsTeam.CounterTerrorist) return HookResult.Changed;
			
			_logs.Append(_logs.Player(player), $"dropped weapon: {weapon.DesignerName}");
			return HookResult.Continue;
		}, HookMode.Pre);
	}

	[GameEventHandler]
	public void OnItemPickup(EventItemPickup @event, GameEventInfo info)
	{
		var player = @event.Userid;
		if (!player.IsReal())
			return;
		
		if (!player.IsValid)
			return;
		
		if (player.Team != CsTeam.Terrorist) return;


		Server.NextFrame(() =>
		{
			var pawn = player.PlayerPawn.Value;
			if (pawn == null || !pawn.IsValid) return;
			var weaponServices = pawn.WeaponServices;
			if (weaponServices == null) return;

			foreach (var weaponHandle in weaponServices.MyWeapons.Where(handle => handle.IsValid && handle.Value != null).ToList())
			{
				if (!weaponHandle.Value.DesignerName.Equals(@event.Item)) continue;

				var owner = weaponHandle.Value.OwnerEntity.Value.As<CCSPlayerController>();
				if (owner.Team != CsTeam.CounterTerrorist) return;
				_logs.Append(_logs.Player(player), $"picked up an {_logs.Player(owner)}s item: {weaponHandle.Value.DesignerName}");
			}
		});

	}

	[EntityOutputHook("func_button", "OnPressed")]
	public HookResult OnButtonPressed(CEntityIOOutput output, string name, CEntityInstance activator,
		CEntityInstance caller, CVariant value, float delay)
	{
		if (!activator.TryGetController(out var player))
			return HookResult.Continue;

		CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);


		_logs.Append(_logs.Player(player), $"pressed a button: {ent.Entity?.Name ?? "Unlabeled"} -> {output?.Connections?.TargetDesc ?? "None"}");
		return HookResult.Continue;
	}

	[EntityOutputHook("func_breakable", "OnBreak")]
	public HookResult OnBreakableBroken(CEntityIOOutput output, string name, CEntityInstance activator,
		CEntityInstance caller, CVariant value, float delay)
	{
		if (!activator.TryGetController(out var player))
			return HookResult.Continue;

		CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);


		_logs.Append(_logs.Player(player), $"broke an entity: {ent.Entity?.Name ?? "Unlabeled"} -> {output?.Connections?.TargetDesc ?? "None"}");
		return HookResult.Continue;
	}
}
