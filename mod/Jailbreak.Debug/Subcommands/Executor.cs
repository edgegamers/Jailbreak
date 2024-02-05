using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Debug.Subcommands;

public interface Executor
{
    void HandleExecution(CCSPlayerController? executor, WrappedInfo info);
}