using CounterStrikeSharp.API.Core;

namespace api.plugin.models;

public interface ICookie
{
    Task<string?> Get(CCSPlayerController player);
    void Set(CCSPlayerController player, string value);
}