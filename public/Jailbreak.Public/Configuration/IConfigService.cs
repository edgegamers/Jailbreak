namespace Jailbreak.Public.Configuration;

public interface IConfigService {
  public const string ConfigPath = "jailbreak.json";

  /// <summary>
  ///   Get the configuration object with the provided name
  /// </summary>
  /// <param name="path"></param>
  /// <param name="failOnDefault">If the configuration service would return the default value, fail instead. Loudly.</param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  T Get<T>(string path, bool failOnDefault = false) where T : class, new();
}