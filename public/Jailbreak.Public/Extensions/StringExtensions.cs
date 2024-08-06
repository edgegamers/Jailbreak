using System.Text;

namespace Jailbreak.Public.Extensions;

public static class StringExtensions {
  public static string Sanitize(this string unknown) {
    return unknown.Replace("<", "&lt;");
  }

  public static string Repeat(this string stringToRepeat, int repeat) {
    var builder = new StringBuilder(repeat * stringToRepeat.Length);
    for (int i = 0; i < repeat; i++) { builder.Append(stringToRepeat); }

    return builder.ToString();
  }
}