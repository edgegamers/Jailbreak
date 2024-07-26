using CounterStrikeSharp.API.Core;

namespace api.plugin;

public class PlayerInfo
{
    public string? authorizedSteamId;
    public uint index;
    public string? ipAddress;
    public string name;
    public ulong unauthorizedSteamId;
    public int? userId;

    public PlayerInfo(CCSPlayerController player)
    {
        userId = player.UserId;
        index = player.Index;
        authorizedSteamId = player.AuthorizedSteamID?.SteamId64.ToString();
        unauthorizedSteamId = player.SteamID;
        name = player.PlayerName;
        ipAddress = player.IpAddress;
    }
}