using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Formatting.Objects;

public class PlayerFormatObject : FormatObject
{
    private readonly CCSPlayerController _player;

    public PlayerFormatObject(CCSPlayerController player)
    {
        _player = player;
    }

    public override string ToChat()
    {
        return $"{ChatColors.Yellow}{_player.PlayerName}";
    }

    public override string ToPanorama()
    {
        return _player.PlayerName.Sanitize();
    }

    public override string ToPlain()
    {
        return _player.PlayerName;
    }
}