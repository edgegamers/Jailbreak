﻿using System.Drawing;

namespace Jailbreak.RTD.Rewards;

public class TransparentReward()
  : ColorReward(Color.FromArgb(128, 255, 255, 255), false) {
  public override string Name => "Spawn Transparent";

  public override string Description
    => "You won spawning transparent next round.";
}