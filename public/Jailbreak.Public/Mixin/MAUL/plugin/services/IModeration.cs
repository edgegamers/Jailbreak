using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

public interface IModeration
{
    void KickPlayer(CCSPlayerController player, string reason, CCSPlayerController? admin = null);

    void BanPlayer(CCSPlayerController player, string reason, int time, CCSPlayerController? admin = null,
        bool push = true);

    bool IsPlayerInLimbo(CCSPlayerController? player);
}