using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Rebel.Bomb;

public class BombNotificationsBehavior : IPluginBehavior, IBombResultHook
{
	private IBombNotifications _notifications;

	public BombNotificationsBehavior(IBombNotifications notifications)
	{
		_notifications = notifications;
	}

	[GameEventHandler]
	public HookResult OnPlayerPickupC4(EventBombPickup @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player == null || !player.IsValid) { return HookResult.Continue; }

		CPlayer_WeaponServices? weaponServices = player.PlayerPawn?.Value?.WeaponServices;
		if (weaponServices == null) { return HookResult.Continue; }

		if (weaponServices.MyWeapons.Last()?.Value == null) { return HookResult.Continue; }
		CC4 bombEntity = new CC4(weaponServices.MyWeapons.Last()!.Value!.Handle); // The last item in the weapons list is the last item the player picked up, apparently.

		if (bombEntity.Globalname == BombBehavior.C4_NAME)
			_notifications.BOMB_PICKUP
				.ToPlayerChat(player)
				.ToPlayerCenter(player);

		return HookResult.Continue;

	}

	public void OnDetonation(BombResult result)
	{
		_notifications.PLAYER_RESULTS(result.Damage, result.Kills)
			.ToAllChat();
	}
}
