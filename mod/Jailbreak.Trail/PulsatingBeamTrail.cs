using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Trail;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Trail;

public class PulsatingBeamTrail : AbstractTrail<BeamTrailSegment> {
  private readonly BasePlugin plugin;
  private readonly Timer timer;

  public PulsatingBeamTrail(BasePlugin plugin, float lifetime = 20,
    int maxPoints = 100, float updateRate = 0.25f,
    Func<float, float>? transform = null) : base(lifetime, maxPoints) {
    this.plugin = plugin;

    timer = plugin.AddTimer(updateRate, Update,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
  }

  protected void Update() {
    var i = 0;
    foreach (var segment in Segments.Reverse()) {
      var line  = segment.GetLine();
      var width = MathF.Sin(-Server.CurrentTime * 3f + i * 1.5f) * 5f - 2.5f;
      if (width < 0)
        line.SetColor(Color.FromArgb(0, 0, 0, 0));
      else
        line.SetColor(Color.FromArgb(
          (int)Math.Clamp(width / 2.5f * 255, 0, 255), 255, 255, 255));
      line.SetWidth(width);
      line.Update();
      i += 1;
    }
  }

  public override void Kill() {
    base.Kill();
    timer.Kill();
  }

  public static PulsatingBeamTrail? FromTrail<T>(BasePlugin plugin,
    AbstractTrail<T> trail, float updateRate = 0.25f,
    Func<float, float>? transform = null) where T : ITrailSegment {
    var beamTrail = new PulsatingBeamTrail(plugin, trail.Lifetime,
      trail.MaxPoints, updateRate, transform);
    foreach (var segment in trail)
      beamTrail.Segments.Add(
        beamTrail.CreateSegment(segment.GetStart(), segment.GetEnd()));

    return beamTrail;
  }

  public override BeamTrailSegment CreateSegment(Vector start, Vector end) {
    var beam = new BeamLine(plugin, start, end);
    return new BeamTrailSegment(beam);
  }
}