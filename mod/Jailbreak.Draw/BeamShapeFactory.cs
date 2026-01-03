using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Draw.Shapes;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw;

public class BeamShapeFactory(IBeamShapeRegistry registry) : IBeamShapeFactory {
  private BasePlugin plugin = null!;
  public void Start(BasePlugin basePlugin) { plugin = basePlugin; }

  public BeamedPolylineShape CreateShape(Vector position,
    BeamShapeType shapeType, float? radius = null, float? width = null) {
    var def = registry.Get(shapeType);

    return new BeamedPolylineShape(plugin, position, def, radius, width);
  }
}