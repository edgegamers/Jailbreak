using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Debug.Subcommands;

public class WrappedInfo(CommandInfo info)
{
    public readonly CommandInfo Info = info;

    public CCSPlayerController? CallingPlayer => Info.CallingPlayer;

    public IntPtr Handle => Info.Handle;

    public int ArgCount => Info.ArgCount - 1;

    public string ArgString => Info.ArgString[(Info.ArgString.IndexOf(' ') + 1)..];

    public string GetCommandString => Info.GetCommandString[(Info.GetCommandString.IndexOf(' ') + 1)..];

    public string ArgByIndex(int index)
    {
        return Info.ArgByIndex(index + 1);
    }

    public string GetArg(int index)
    {
        return Info.GetArg(index + 1);
    }

    public void ReplyToCommand(string message)
    {
        Info.ReplyToCommand(message);
    }
}