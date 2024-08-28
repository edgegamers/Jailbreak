﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class BombReward(IC4Service bombService) : IRTDReward {
  public string Name => "Bomb";
  public string Description => "You won the bomb next round.";

  public bool PrepareReward(int userid) {
    bombService.DontGiveC4NextRound();
    return true;
  }

  public bool CanGrantReward(CCSPlayerController player) {
    return player.Team == CsTeam.Terrorist;
  }

  public bool GrantReward(CCSPlayerController player) {
    bombService.TryGiveC4ToPlayer(player);
    return true;
  }
}