using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Objects;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Formatting.Core;

public abstract class FormatObject
{

	/// <summary>
	/// Output this format object compatible with CS2 chat formatting.
	/// </summary>
	/// <returns></returns>
	public virtual string ToChat()
		=> ToPlain();

	/// <summary>
	/// Output this format object in a panorama-compatible format.
	/// </summary>
	/// <returns></returns>
	public virtual string ToPanorama()
		=> ToPlain().Sanitize();

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
