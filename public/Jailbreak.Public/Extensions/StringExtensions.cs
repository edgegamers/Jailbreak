namespace Jailbreak.Public.Extensions;

public static class StringExtensions
{
	public static string Sanitize(this string unknown)
	{
		return unknown
			.Replace("<", "&lt;");
	}
}
