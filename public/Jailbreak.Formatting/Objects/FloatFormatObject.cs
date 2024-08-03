using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class FloatFormatObject(float value, char chatColor = '\x09')
  : FormatObject {
  public float Value { get; } = value;

  public override string ToChat() { return $"{chatColor}{Value:F2}"; }

  public override string ToPanorama() { return Value.ToString("F2"); }

  public override string ToPlain() { return Value.ToString("F2"); }
}