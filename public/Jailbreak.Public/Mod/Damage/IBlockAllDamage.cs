using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Damage;

public interface IBlockUserDamage
{
    HookResult BlockUserDamage(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        var attacker = @event.Attacker;
        if (player == null || !player.IsReal())
            return HookResult.Continue;

        if (!ShouldBlockDamage(player, attacker, @event))
        {
            return HookResult.Continue;
        }
        if (player.PlayerPawn.IsValid)
        {
            CCSPlayerPawn playerPawn = player.PlayerPawn.Value!;
            playerPawn.Health = playerPawn.LastHealth;
        }
        @event.DmgArmor = 0;
        @event.DmgHealth = 0;
        return HookResult.Stop;
    }

    bool ShouldBlockDamage(CCSPlayerController victim, CCSPlayerController? attacker, EventPlayerHurt @event);
}