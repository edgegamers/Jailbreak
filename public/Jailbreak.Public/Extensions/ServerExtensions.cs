using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class ServerExtensions
{
    public static void PrintToCenterAll(string message)
    {
        VirtualFunctions.ClientPrintAll(HudDestination.Center, message, 0, 0, 0, 0);
    }
}