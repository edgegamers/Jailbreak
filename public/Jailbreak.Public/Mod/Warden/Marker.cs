using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CS2DrawShared;
using CS2TraceRay.Struct;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden.Enums;

namespace Jailbreak.Public.Mod.Warden;

/// <summary>
/// A Marker is a custom shape + a vertical beam, bundled as a single unit.
/// The beam runs from 10 units above the origin to 130 units above it.
/// Call Place() to spawn, Cancel() to remove both at once.
/// </summary>
public sealed class Marker(IDrawService draw, Vector origin, CGameTrace? trace,
  MarkerShapeType shapeType, float radius, int particles = 33) {
  private Vector origin = origin;
  private float radius = radius;

  private Color? color;
  private float lifetime;
  private bool infinite = true;

  private IDrawHandle? shapeHandle;
  private IDrawHandle? beamHandle;

  public bool IsPlaced
    => shapeHandle?.IsAlive == true || beamHandle?.IsAlive == true;

  public MarkerShapeType ShapeType { get; } = shapeType;

  public Marker Color(Color markerColor) {
    color = markerColor;
    return this;
  }

  public Marker WithLifetime(float seconds) {
    lifetime = seconds;
    infinite = false;
    return this;
  }

  public Marker Infinite() {
    infinite = true;
    lifetime = 0f;
    return this;
  }

  /// <summary>
  /// Cancel and re-place at a new world position.
  /// </summary>
  public Marker Move(Vector newOrigin) {
    origin = newOrigin;
    return Place();
  }

  /// <summary>
  /// Update the radius and re-place.
  /// </summary>
  public Marker Resize(float newRadius) {
    radius = newRadius;
    return Place();
  }

  /// <summary>
  /// Update the color and re-place.
  /// </summary>
  public Marker Recolor(Color newColor) {
    color = newColor;
    return Place();
  }

  /// <summary>
  /// Spawns the shape and beam. Safe to call again after Cancel() to re-place.
  /// </summary>
  public Marker Place() {
    // cancel any existing spawn before re-placing
    Cancel();

    var setup = new MarkerShapeSetup(ShapeType, radius, particles);
    
    var tracePos = new Vector(trace?.EndPos.X, trace?.EndPos.Y,
      trace?.EndPos.Z);

    var spawnLocation =
      new Vector(origin.X, origin.Y, origin.Z + 32); // fallback if trace is bad

    if (positionsMatch(origin, tracePos)) {
      var rawNormal = trace?.Normal.ToCsVector();

      if (rawNormal != null) {
        var normal = rawNormal.Normalize();

        const float maxAllowedSlopeDegrees = 60f;
        const float minNormalZ             = 0.5f;

        if (normal.Z >= minNormalZ) {
          var zOffset = getMinimumLift(normal, 60);
          spawnLocation.Z = origin.Z + zOffset + 4;
        } else {
          spawnLocation = new Vector(origin.X, origin.Y, origin.Z);
        }
      }
    }

    // shape
    var shapeBuilder = draw.Custom(spawnLocation, setup).Particles(particles);

    if (color.HasValue) shapeBuilder.Color(color.Value);
    if (infinite)
      shapeBuilder.Infinite();
    else
      shapeBuilder.WithLifetime(lifetime);

    shapeHandle = shapeBuilder.Draw();

    // beam — 10 units above origin to 130 units above origin
    var beamFrom = new Vector(origin.X, origin.Y, origin.Z + 105f);
    var beamTo   = new Vector(origin.X, origin.Y, origin.Z + 130f);

    var beamBuilder = draw.Beam(beamFrom, beamTo);

    if (color.HasValue) beamBuilder.Color(color.Value);
    if (infinite)
      beamBuilder.Infinite();
    else
      beamBuilder.WithLifetime(lifetime);

    beamHandle = beamBuilder.Draw();

    return this;
  }

  /// <summary>
  /// Cancels both the shape and the beam.
  /// </summary>
  public void Cancel() {
    if (shapeHandle?.IsAlive == true) shapeHandle.Cancel();
    if (beamHandle?.IsAlive == true) beamHandle.Cancel();
  }
  
  private static float getMinimumLift(Vector normal, float radius) {
    var n  = normal.Normalize();
    var up = new Vector(0f, 0f, 1f);

    var dot = MathF.Abs(n.Dot(up));
    dot = Math.Clamp(dot, 0f, 1f);

    return radius * MathF.Sqrt(1f - dot * dot) + 5;
  }

  private static bool positionsMatch(Vector a, Vector b, float tolerance = 1f) {
    var delta = a - b;
    return delta.LengthSqr() <= tolerance * tolerance;
  }
}