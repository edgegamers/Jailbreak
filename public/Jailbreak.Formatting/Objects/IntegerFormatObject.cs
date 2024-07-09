using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class IntegerFormatObject(int value, char chatColor = '\x09')
  : FormatObject {
  public int Value { get; } = value;

  public override string ToChat() { return $"{chatColor}{Value.ToString()}"; }

  public override string ToPanorama() { return Value.ToString(); }

  public override string ToPlain() { return Value.ToString(); }
}