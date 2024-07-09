using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Formatting.Views.Logging;

public interface IRichLogService : ILogService {
  void Append(params FormatObject[] objects);

  FormatObject Player(CCSPlayerController playerController);
}