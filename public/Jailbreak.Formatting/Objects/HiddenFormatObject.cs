using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

public class HiddenFormatObject : FormatObject
{

	public FormatObject _value;

	public HiddenFormatObject(FormatObject value)
	{
		_value = value;
	}

	public bool Plain { get; set; } = true;

	public bool Panorama { get; set; } = true;

	public bool Chat { get; set; } = true;

	public override string ToChat()
	{
		if (Chat)
			return _value.ToChat();
		return String.Empty;
	}

	public override string ToPanorama()
	{
		if (Panorama)
			return _value.ToPanorama();
		return String.Empty;	}

	public override string ToPlain()
	{
		if (Plain)
			return _value.ToPlain();
		return String.Empty;
	}
}
