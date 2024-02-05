using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class HiddenFormatObject : FormatObject
{
    public FormatObject Value;

    public HiddenFormatObject(FormatObject value)
    {
        Value = value;
    }

    public bool Plain { get; set; } = true;

    public bool Panorama { get; set; } = true;

    public bool Chat { get; set; } = true;

    public override string ToChat()
    {
        if (Chat)
            return Value.ToChat();
        return string.Empty;
    }

    public override string ToPanorama()
    {
        if (Panorama)
            return Value.ToPanorama();
        return string.Empty;
    }

    public override string ToPlain()
    {
        if (Plain)
            return Value.ToPlain();
        return string.Empty;
    }
}