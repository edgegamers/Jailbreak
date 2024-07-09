using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Logs;

public interface ILogService {
  void Append(string message);
  IEnumerable<string> GetMessages();
  void Clear();
  void PrintLogs(CCSPlayerController? player);
}