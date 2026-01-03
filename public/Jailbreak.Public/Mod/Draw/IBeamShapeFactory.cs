using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Public.Mod.Draw;

public interface IBeamShapeFactory : IPluginBehavior {
  BeamedPolylineShape CreateShape(Vector position,
    BeamShapeType shapeType, float? radius = null, float? width = null);
}