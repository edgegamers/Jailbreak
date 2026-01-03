using System.Drawing;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///   Registry for retrieving beam shape definitions by type.
/// </summary>
public interface IBeamShapeRegistry {
  /// <summary>
  ///   Gets the shape definition for the specified type.
  /// </summary>
  /// <param name="type">The shape type to retrieve.</param>
  /// <returns>The shape definition.</returns>
  /// <exception cref="ArgumentException">If the shape type is not registered.</exception>
  IBeamShapeDefinition Get(BeamShapeType type);

  /// <summary>
  ///   Attempts to get the shape definition for the specified type.
  /// </summary>
  /// <param name="type">The shape type to retrieve.</param>
  /// <param name="definition">The shape definition if found, null otherwise.</param>
  /// <returns>True if the shape was found, false otherwise.</returns>
  bool TryGet(BeamShapeType type, out IBeamShapeDefinition? definition);

  /// <summary>
  ///   Gets all registered shape types.
  /// </summary>
  /// <returns>An enumerable of all registered shape types.</returns>
  IEnumerable<BeamShapeType> GetAllTypes();

  /// <summary>
  ///  Get all registered colors.
  /// </summary>
  /// <returns></returns>
  Dictionary<string, Color> GetAllColors();
}