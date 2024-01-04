using System.Text.Json;
using System.Text.Json.Nodes;

using CounterStrikeSharp.API;

using Jailbreak.Public.Configuration;

using Microsoft.Extensions.Logging;


namespace Jailbreak.Config;

public class ConfigService : IConfigService
{
	private ILogger<ConfigService> _logger;

	public ConfigService(ILogger<ConfigService> logger)
	{
		_logger = logger;
	}

	private T Fail<T>(bool fail, string message)
		where T: class, new()
	{
		//	We would be returning default.
		//	Check if caller wants us to cry and scream instead.
		if (fail)
			throw new InvalidOperationException(message);

		_logger.LogWarning("[Config] Tripped load fail state with message: {@Message}", message);

		return new T();
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="path"></param>
	/// <param name="fail"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T Get<T>(string path, bool fail = false)
		where T : class, new()
	{
		var jsonPath = Path.Combine(Server.GameDirectory, IConfigService.CONFIG_PATH);

		if (!File.Exists(jsonPath))
			return Fail<T>(fail, "Config file does not exist");

		var jsonText = File.ReadAllText(jsonPath);

		var jsonObject = JsonObject.Parse(jsonText);
		if (jsonObject == null)
			return Fail<T>(fail, $"Unable to parse configuration file at {jsonPath}");

		var configObject = jsonObject[path];
		if (configObject == null)
			return Fail<T>(fail, $"Unable to navigate to config section {path}");

		var config = configObject.Deserialize<T>();
		if (config == null)
			return Fail<T>(fail, $"Unable to deserialize ({configObject.ToJsonString()}) into {typeof(T).FullName}.");

		return config;
	}
}
