using System.Drawing;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenMarkerSettings : IPluginBehavior {
  ValueTask<MarkerSettings> GetForWardenAsync(ulong steamId);
  Task SetTypeAsync(ulong steamId, BeamShapeType type);
  Task SetColorAsync(ulong steamId, string colorKey);
  void Invalidate(ulong steamId);
}

public readonly record struct MarkerSettings(BeamShapeType Type, Color color);