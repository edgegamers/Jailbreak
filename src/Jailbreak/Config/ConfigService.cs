﻿using System.Text.Json;
using System.Text.Json.Nodes;
using CounterStrikeSharp.API;
using Jailbreak.Public.Configuration;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Config;

/// <summary>
/// A service to load and parse configuration files.
/// </summary>
public class ConfigService : IConfigService
{
    private readonly ILogger<ConfigService> _logger;

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="logger"></param>
    public ConfigService(ILogger<ConfigService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fail"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T>(string path, bool fail = false)
        where T : class, new()
    {
        var jsonPath = Path.Combine(Server.GameDirectory, IConfigService.ConfigPath);

        if (!File.Exists(jsonPath))
            return Fail<T>(fail, "Config file does not exist");

        var jsonText = File.ReadAllText(jsonPath);

        var jsonObject = JsonNode.Parse(jsonText);
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

    private T Fail<T>(bool fail, string message)
        where T : class, new()
    {
        //	We would be returning default.
        //	Check if caller wants us to cry and scream instead.
        if (fail)
            throw new InvalidOperationException(message);

        _logger.LogWarning("[Config] Tripped load fail state with message: {@Message}", message);

        return new T();
    }
}