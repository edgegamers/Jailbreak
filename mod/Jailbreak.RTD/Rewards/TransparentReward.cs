using System.Drawing;

namespace Jailbreak.RTD.Rewards;

public class TransparentReward()
  : ColorReward(Color.FromArgb(70, 255, 255, 255), false) {
  public override string Name => "Spawn Transparent";

  public override string Description
    => "You will spawn transparent next round.";

  public override bool Enabled => false;
}