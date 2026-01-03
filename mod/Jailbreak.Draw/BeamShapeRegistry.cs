using System.Drawing;
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
  
  private readonly Dictionary<string, Color> colors =
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
    register(new JellyBeanShapeDefinition());
    
    register(Color.Red);
    register(Color.Green);
    register(Color.Blue);
    register(Color.Magenta);
    register(Color.Yellow);
    register(Color.Purple);
    register(Color.Pink);
    register(Color.Cyan);
    register(Color.White);
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
  
  public Dictionary<string, Color> GetAllColors() { return colors; }

  private void register(IBeamShapeDefinition definition) {
    shapes[definition.ShapeType] = definition;
  }
  
  private void register(Color color) {
    colors[color.Name] = color;
  }
}