using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class IntegerFormatObject : FormatObject
{
    private readonly char _chatColor;

    public IntegerFormatObject(int value, char chatColor = '\x09')
    {
        Value = value;
        _chatColor = chatColor;
    }

    public int Value { get; }

    public override string ToChat()
    {
        return $"{_chatColor}{Value.ToString()}";
    }

    public override string ToPanorama()
    {
        return Value.ToString();
    }

    public override string ToPlain()
    {
        return Value.ToString();
    }
}