using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace plugin;

public class ActainConfig : BasePluginConfig
{
    [JsonPropertyName("MAUL_URL")] public string? MaulUrl { get; set; }
    [JsonPropertyName("MAUL_SERVER_PORT")] public int MaulServerPort { get; set; }
    [JsonPropertyName("MAUL_API_KEY")] public string? MaulApiKey { get; set; }
    [JsonPropertyName("MAUL_DEBUG")] public bool MaulDebug { get; set; }

    [JsonPropertyName("MAUL_PLAYER_CHAT_FLAG")]
    public string? MaulPlayerChatVisibility { get; set; }

    [JsonPropertyName("EC_HAS_HEIGHTENED_PERMS")] public bool ECHasHeightendPerms { get; set; }

    [JsonPropertyName("Ranks")] public JsonObject? Ranks { get; set; }

    [JsonPropertyName("DBConnectionString")]
    public string? DBConnectionString { get; set; }

    [JsonPropertyName("Tags")] public JsonObject? Tags { get; set; }
    [JsonPropertyName("Advertisements")] public string[] Advertisements { get; set; }

    [JsonPropertyName("AdvertiseSecondPeriod")]
    public int AdvertiseSecondPeriod { get; set; }
}