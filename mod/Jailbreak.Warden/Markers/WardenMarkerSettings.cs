using System.Collections.Concurrent;
using System.Drawing;
using Jailbreak.Public;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;
using Jailbreak.Public.Mod.Warden;
using MAULActainShared.plugin.models;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerSettings(IBeamShapeRegistry registry) : IWardenMarkerSettings {
  private const string TYPE_COOKIE  = "jb_marker_type";
  private const string COLOR_COOKIE = "jb_marker_color";
  
  private readonly ConcurrentDictionary<ulong, MarkerSettings> cache = new();
  private ICookie? typeCookie;
  private ICookie? colorCookie;

  public async ValueTask<MarkerSettings> GetForWardenAsync(ulong steamId) {
    if (cache.TryGetValue(steamId, out var cached))
      return cached;

    // defaults
    var type  = BeamShapeType.CIRCLE;
    var color = Color.White;

    if (!await ensureCookiesAsync())
      return store(steamId, type, color);

    try {
      var typeStr  = await typeCookie!.Get(steamId);
      var colorStr = await colorCookie!.Get(steamId);

      if (!string.IsNullOrWhiteSpace(typeStr))
        type = parseType(typeStr, type);

      if (!string.IsNullOrWhiteSpace(colorStr))
        color = parseColor(colorStr, color);
    } catch {
      // swallow: cookie service can be flaky on map start, keep defaults
    }

    return store(steamId, type, color);
  }

  public void Invalidate(ulong steamId) => cache.TryRemove(steamId, out _);

  private MarkerSettings store(ulong steamId, BeamShapeType type, Color color) {
    var s = new MarkerSettings(type, color);
    cache[steamId] = s;
    return s;
  }

  private BeamShapeType parseType(string value, BeamShapeType fallback) {
    foreach (var t in registry.GetAllTypes())
      if (string.Equals(t.ToFriendlyString(), value, StringComparison.OrdinalIgnoreCase))
        return t;
    
    return Enum.TryParse<BeamShapeType>(value, true, out var parsed) ? parsed : fallback;
  }

  private Color parseColor(string key, Color fallback) {
    var colors = registry.GetAllColors();
    return colors.TryGetValue(key, out var c) ? c : fallback;
  }

  private async ValueTask<bool> ensureCookiesAsync() {
    if (typeCookie != null && colorCookie != null) return true;
    if (API.Actain == null) return false;

    try {
      var svc = API.Actain.getCookieService();
      typeCookie  = await svc.RegClientCookie(TYPE_COOKIE);
      colorCookie = await svc.RegClientCookie(COLOR_COOKIE);
      return typeCookie != null && colorCookie != null;
    } catch {
      return false;
    }
  }
}