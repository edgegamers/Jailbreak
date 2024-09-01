namespace Jailbreak.Public.Extensions;

public static class StringExtensions {
  public static string Sanitize(this string unknown) {
    return unknown.Replace("<", "&lt;");
  }

  public static bool IsVowel(this char c) {
    return "aeiouAEIOU".IndexOf(c) >= 0;
  }

  // This is a comment => This Is A Comment
  public static string ToTitleCase(this string unknown) {
    var words = unknown.Split(' ');
    for (var i = 0; i < words.Length; i++) {
      if (words[i].Length == 0) continue;
      var firstLetter = words[i][0];
      words[i] = words[i][1..];
      words[i] = firstLetter.ToString().ToUpper() + words[i].ToLower();
    }

    return string.Join(' ', words);
  }
}