using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.LastRequest;

public class LastRequestCommand : IPluginBehavior
{
   [ConsoleCommand("css_lr", "Start a last request as a prisoner")]
   [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
   public void Command_LastRequest(CCSPlayerController? executor, CommandInfo info)
   {
      
   }
}