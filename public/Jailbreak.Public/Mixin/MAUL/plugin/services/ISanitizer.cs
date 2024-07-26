using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

public interface ISanitizer
{
    string FilterChat(CCSPlayerController sender, string msg);
    string FilterName(string name, int userId);
}