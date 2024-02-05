using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Debug.Subcommands;

public class MarkRebel
{
    public void Command_MarkRebel(CCSPlayerController? executor, CommandInfo info)
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