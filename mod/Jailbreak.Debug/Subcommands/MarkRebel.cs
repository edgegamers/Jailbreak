using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Debug.Subcommands;

// css_markrebel [player] <duration>
public class MarkRebel : Executor
{

    public MarkRebel()
    {
        
    }

    public void HandleExecution(CCSPlayerController? executor, WrappedInfo info)
    {
        if (executor == null)
        {
            return;
        }

        if (info.ArgCount == 1)
        {
            // asdf    
        }
    }
}