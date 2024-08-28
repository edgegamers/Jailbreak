using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class TransparentReward()
  : ColorReward(Color.FromArgb(128, 255, 255, 255), false) {
  public override string Name => "Spawn Transparent";

  public override string Description
    => "You won spawning transparent next round.";
}