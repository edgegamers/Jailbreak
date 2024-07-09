using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class StringFormatObject : FormatObject {
  private readonly char _chatColor;

  public StringFormatObject(string value, char chatColor = '\x01') {
    Value      = value;
    _chatColor = chatColor;
  }

  public string Value { get; }

  public override string ToChat() { return $"{_chatColor}{Value}"; }

  public override string ToPanorama() { return Value; }

  public override string ToPlain() { return Value; }
}