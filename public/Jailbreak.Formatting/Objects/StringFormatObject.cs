using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class StringFormatObject(string value, char chatColor = '\x08')
  : FormatObject {
  public string Value { get; } = value;

  public override string ToChat() { return $"{chatColor}{Value}"; }

  public override string ToPanorama() { return Value; }

  public override string ToPlain() { return Value; }
}