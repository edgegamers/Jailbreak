using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Objects;

namespace Jailbreak.Formatting.Core;

public abstract class FormatObject
{

	/// <summary>
	/// Output this format object compatible with CS2 chat formatting.
	/// </summary>
	/// <returns></returns>
	public abstract string ToChat();

	/// <summary>
	/// Output this format object in a panorama-compatible format.
	/// </summary>
	/// <returns></returns>
	public abstract string ToPanorama();

	/// <summary>
	/// Output plaintext
	/// </summary>
	/// <returns></returns>
	public abstract string ToPlain();


	public static implicit operator FormatObject(string value)
		=> new StringFormatObject(value);

	public static implicit operator FormatObject(CCSPlayerController value)
		=> new PlayerFormatObject(value);

	public static FormatObject FromObject(object value) => new StringFormatObject(value.ToString());
}
