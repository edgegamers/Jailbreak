﻿using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Trail;

public class BeamTrailSegment : VectorTrailSegment {
  private readonly BeamLine line;

  public BeamTrailSegment(BeamLine line) : base(line.Position, line.End) {
    this.line = line;
    line.Draw();
  }

  public override void Remove() { line.Remove(); }
  public BeamLine GetLine() { return line; }
}