using System.Drawing;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden.Enums;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenMarkerSettings : IPluginBehavior {
  MarkerSettings? GetCachedSettings(ulong steamId);
  Task EnsureCachedAsync(ulong steamId);
  Task SetTypeAsync(ulong steamId, MarkerShapeType type);
  Task SetColorAsync(ulong steamId, string colorKey);
  void Invalidate(ulong steamId);
}

public readonly record struct MarkerSettings(MarkerShapeType Type, Color color);