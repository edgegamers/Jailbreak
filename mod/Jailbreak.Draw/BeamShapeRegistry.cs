using Jailbreak.Draw.Shapes;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw;

/// <summary>
///   Registry for all available beam shape definitions.
///   Maps BeamShapeType enum values to their corresponding shape definitions.
/// </summary>
public class BeamShapeRegistry : IBeamShapeRegistry {
  private readonly Dictionary<BeamShapeType, IBeamShapeDefinition> shapes =
    new();

  public BeamShapeRegistry() {
    // Register all built-in shapes
    register(new CircleShapeDefinition());
    register(new SquareShapeDefinition());
    register(new DiamondShapeDefinition());
    register(new StarShapeDefinition());
    register(new TriangleShapeDefinition());
    register(new PentagonShapeDefinition());
    register(new HexagonShapeDefinition());
    register(new HeartShapeDefinition());
    register(new AmongUsShapeDefinition());
  }

  public IBeamShapeDefinition Get(BeamShapeType type) {
    return shapes.TryGetValue(type, out var definition) ?
      definition :
      throw new ArgumentException(
        $"No shape definition registered for type: {type}");
  }

  public bool TryGet(BeamShapeType type, out IBeamShapeDefinition? definition) {
    return shapes.TryGetValue(type, out definition);
  }

  public IEnumerable<BeamShapeType> GetAllTypes() { return shapes.Keys; }

  private void register(IBeamShapeDefinition definition) {
    shapes[definition.ShapeType] = definition;
  }
}