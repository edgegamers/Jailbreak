using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class StringFormatObject : FormatObject
{
	private string _value;
	private char _chatColor;

	public StringFormatObject(string value, char chatColor = '\x01')
	{
		_value = value;
		_chatColor = chatColor;
	}

	public string Value => _value;

	public override string ToChat()
		=> $"{_chatColor}{Value}";

	public override string ToPanorama()
		=> Value;

	public override string ToPlain()
		=> Value;
}
