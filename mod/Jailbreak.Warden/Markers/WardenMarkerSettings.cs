using System.Collections.Concurrent;
using System.Drawing;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;
using Jailbreak.Public.Mod.Warden;
using MAULActainShared.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerSettings(IBeamShapeRegistry registry)
  : IWardenMarkerSettings, IPluginBehavior {
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

  public async ValueTask<MarkerSettings> GetForWardenAsync(ulong steamId) {
    if (cache.TryGetValue(steamId, out var cached)) return cached;

    // defaults
    var type = BeamShapeType.CIRCLE;
    var color = Color.White;

    if (typeCookie == null || colorCookie == null)
      return store(steamId, type, color);

    try {
      var typeStr = await typeCookie.Get(steamId);
      var colorStr = await colorCookie.Get(steamId);

      if (!string.IsNullOrWhiteSpace(typeStr)) type = parseType(typeStr, type);

      if (!string.IsNullOrWhiteSpace(colorStr))
        color = parseColor(colorStr, color);
    } catch {
      // swallow: cookie service can be flaky on map start, keep defaults
    }

    return store(steamId, type, color);
  }

  public async Task SetTypeAsync(ulong steamId, BeamShapeType type) {
    if (typeCookie == null) return;

    var value = type.ToFriendlyString();
    await typeCookie.Set(steamId, value);

    // Update cache
    if (cache.TryGetValue(steamId, out var current)) {
      cache[steamId] = current with { Type = type };
    } else {
      await populateCache(steamId);
    }
  }

  public async Task SetColorAsync(ulong steamId, string colorKey) {
    if (colorCookie == null) return;

    await colorCookie.Set(steamId, colorKey);

    // Update cache
    if (cache.TryGetValue(steamId, out var current)) {
      var color = parseColor(colorKey, current.color);
      cache[steamId] = current with { color = color };
    } else {
      await populateCache(steamId);
    }
  }

  public void Invalidate(ulong steamId) => cache.TryRemove(steamId, out _);

  private async Task populateCache(ulong steamId) {
    if (typeCookie == null || colorCookie == null) return;

    try {
      var typeStr = await typeCookie.Get(steamId);
      var colorStr = await colorCookie.Get(steamId);

      var type = BeamShapeType.CIRCLE;
      var color = Color.White;

      if (!string.IsNullOrWhiteSpace(typeStr)) type = parseType(typeStr, type);
      if (!string.IsNullOrWhiteSpace(colorStr))
        color = parseColor(colorStr, color);

      cache[steamId] = new MarkerSettings(type, color);
    } catch {
      // swallow
    }
  }

  private MarkerSettings store(ulong steamId, BeamShapeType type, Color color) {
    var s = new MarkerSettings(type, color);
    cache[steamId] = s;
    return s;
  }

  private BeamShapeType parseType(string value, BeamShapeType fallback) {
    foreach (var t in registry.GetAllTypes())
      if (string.Equals(t.ToFriendlyString(), value,
        StringComparison.OrdinalIgnoreCase))
        return t;

    return Enum.TryParse<BeamShapeType>(value, true, out var parsed) ?
      parsed :
      fallback;
  }

  private Color parseColor(string key, Color fallback) {
    var colors = registry.GetAllColors();
    return colors.TryGetValue(key, out var c) ? c : fallback;
  }

  private void TryLoadCookies() {
    Task.Run(async () => {
      if (API.Actain != null) {
        var svc = API.Actain.getCookieService();
        typeCookie = await svc.RegClientCookie(TYPE_COOKIE);
        colorCookie = await svc.RegClientCookie(COLOR_COOKIE);
      }
    });
  }
}