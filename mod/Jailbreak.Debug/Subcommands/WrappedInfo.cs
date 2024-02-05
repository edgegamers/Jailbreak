using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Debug.Subcommands;

public class WrappedInfo
{
    public readonly CommandInfo info;

    public WrappedInfo(CommandInfo info)
    {
        this.info = info;
    }

    public CCSPlayerController? CallingPlayer => info.CallingPlayer;

    public IntPtr Handle => info.Handle;

    public int ArgCount => info.ArgCount - 1;

    public string ArgString => info.ArgString[(info.ArgString.IndexOf(' ') + 1)..];

    public string GetCommandString => info.GetCommandString[(info.GetCommandString.IndexOf(' ') + 1)..];

    public string ArgByIndex(int index)
    {
        return info.ArgByIndex(index + 1);
    }

    public string GetArg(int index)
    {
        return info.GetArg(index + 1);
    }

    public void ReplyToCommand(string message, bool console = false)
    {
        info.ReplyToCommand(message, console);
    }
}