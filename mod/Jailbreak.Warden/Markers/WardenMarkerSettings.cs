using System.Collections.Concurrent;
using System.Drawing;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using MAULActainShared.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden.Enums;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerSettings : IWardenMarkerSettings, IPluginBehavior {
  private const string TYPE_COOKIE = "jb_marker_type";
  private const string COLOR_COOKIE = "jb_marker_color";

  private readonly ConcurrentDictionary<ulong, MarkerSettings> cache = new();
  private ICookie? typeCookie;
  private ICookie? colorCookie;
  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    TryLoadCookies();
    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
  }

  private void OnMapStart(string mapname) {
    // Attempt to load the cookies OnMapStart if they fail to load on plugin start
    // This can happen if the MAUL plugin is loaded *after* this plugin
    if (typeCookie == null || colorCookie == null)
      TryLoadCookies();
    else
      plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }

  public MarkerSettings? GetCachedSettings(ulong steamId) {
    return cache.TryGetValue(steamId, out var cached) ? cached : null;
  }

  public async Task EnsureCachedAsync(ulong steamId) {
    if (cache.ContainsKey(steamId)) return;
    await populateCache(steamId);
  }

  public async Task SetTypeAsync(ulong steamId, MarkerShapeType type) {
    if (typeCookie == null) return;

    var value = type.ToDisplayName();
    await typeCookie.Set(steamId, value);

    // Refresh cache from cookies
    await populateCache(steamId);
  }

  public async Task SetColorAsync(ulong steamId, string colorKey) {
    if (colorCookie == null) return;

    await colorCookie.Set(steamId, colorKey);

    // Refresh cache from cookies
    await populateCache(steamId);
  }

  public void Invalidate(ulong steamId) => cache.TryRemove(steamId, out _);

  private async Task populateCache(ulong steamId) {
    if (typeCookie == null || colorCookie == null) {
      // Store defaults if cookies aren't ready
      cache[steamId] = new MarkerSettings(MarkerShapeType.CIRCLE, Color.White);
      return;
    }

    try {
      var typeStr = await typeCookie.Get(steamId);
      var colorStr = await colorCookie.Get(steamId);

      var type  = MarkerShapeType.CIRCLE;
      var color = Color.White;

      if (!string.IsNullOrWhiteSpace(typeStr)) type = parseType(typeStr, type);
      if (!string.IsNullOrWhiteSpace(colorStr))
        color = parseColor(colorStr);

      cache[steamId] = new MarkerSettings(type, color);
    } catch {
      // swallow: store defaults on error
      cache[steamId] = new MarkerSettings(MarkerShapeType.CIRCLE, Color.White);
    }
  }

  private MarkerShapeType parseType(string value, MarkerShapeType fallback) {
    foreach (var t in MarkerShapeTypeExtensions.All())
      if (string.Equals(t.ToDisplayName(), value,
        StringComparison.OrdinalIgnoreCase))
        return t;

    return Enum.TryParse<MarkerShapeType>(value, true, out var parsed) ?
      parsed :
      fallback;
  }

  private Color parseColor(string key) {
    return key.ToMarkerColor().ToColor();
  }

  private void TryLoadCookies() {
    Task.Run(async () => {
      if (API.Actain != null) {
        var svc = API.Actain.getCookieService();
        typeCookie  = await svc.RegClientCookie(TYPE_COOKIE);
        colorCookie = await svc.RegClientCookie(COLOR_COOKIE);
        
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsReal())) {
          _ = populateCache(player.SteamID);
        }
      }
    });
  }
}