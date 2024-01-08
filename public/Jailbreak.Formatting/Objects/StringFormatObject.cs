using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class StringFormatObject : FormatObject
{
	private string _value;

	public StringFormatObject(string value)
	{
		_value = value;
	}

	public string Value => _value;

	public override string ToChat()
		=> $"{ChatColors.White}{Value}";

	public override string ToPanorama()
		=> Value;

	public override string ToPlain()
		=> Value;
}
