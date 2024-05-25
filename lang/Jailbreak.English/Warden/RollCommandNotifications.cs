using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class RollCommandNotifications : IRollCommandNotications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX = new HiddenFormatObject($" {ChatColors.Lime}[{ChatColors.Green}Roll{ChatColors.Lime}]")
    {
        //	Hide in panorama and center text
        Plain = false,
        Panorama = false,
        Chat = true
    };
    
    public IView Roll(int roll)
    {
        return new SimpleView() { PREFIX, $"warden has rolled {roll}!" };
    }
}