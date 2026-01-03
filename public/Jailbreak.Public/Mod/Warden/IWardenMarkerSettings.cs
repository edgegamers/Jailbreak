using System.Drawing;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenMarkerSettings : IPluginBehavior {
  ValueTask<MarkerSettings> GetForWardenAsync(ulong steamId);
  void Invalidate(ulong steamId);
}

public readonly record struct MarkerSettings(BeamShapeType Type, Color color);