using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Debug.Subcommands;

public class WrappedInfo(CommandInfo info, int offset = 1) {
  public readonly CommandInfo Info = info;

  public CCSPlayerController? CallingPlayer => Info.CallingPlayer;

  public IntPtr Handle => Info.Handle;

  public int ArgCount => Info.ArgCount - offset;

  public string ArgString
    => Info.ArgString[(Info.ArgString.IndexOf(' ') + offset)..];

  public string GetCommandString
    => Info.GetCommandString[(Info.GetCommandString.IndexOf(' ') + offset)..];

  public string ArgByIndex(int index) {
    return Info.ArgByIndex(index + offset);
  }

  public string GetArg(int index) { return Info.GetArg(index + offset); }

  public void ReplyToCommand(string message) { Info.ReplyToCommand(message); }
}