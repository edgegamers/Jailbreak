using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Logs;

public interface ILogService
{
    void AddLogMessage(string message);
    ICollection<string> GetLogMessages();
    void ClearLogMessages();
    string FormatPlayer(CCSPlayerController player);
    void PrintLogs(CCSPlayerController? player);
}