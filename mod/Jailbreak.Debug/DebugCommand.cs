using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Debug.Subcommands;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Debug;

// css_debug [subcommand] [args] -> subcommand [args]
/// <summary>
///   The debug command allows for Developers to debug and force certain actions/gamestates.
/// </summary>
public class DebugCommand(IServiceProvider serviceProvider) : IPluginBehavior {
  private readonly Dictionary<string, AbstractCommand> commands = new();
  private BasePlugin? plugin;

  public void Start(BasePlugin parent) {
    plugin = parent;
    commands.Add("markrebel", new MarkRebel(serviceProvider));
    commands.Add("pardon", new Pardon(serviceProvider));
    commands.Add("lr", new Subcommands.LastRequest(serviceProvider, plugin));
    commands.Add("st", new MarkST(serviceProvider));
    commands.Add("lg", new LastGuard(serviceProvider));
  }

  [RequiresPermissions("@css/root")]
  [ConsoleCommand("css_debug", "Debug command for Jailbreak.")]
  public void Command_Debug(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;

    if (info.ArgCount == 1) {
      foreach (var command in commands) info.ReplyToCommand(command.Key);
      return;
    }

    if (!commands.TryGetValue(info.GetArg(1), out var subcommand)) {
      info.ReplyToCommand("Invalid subcommand");
      return;
    }

    subcommand.OnCommand(executor, new WrappedInfo(info));
  }
}