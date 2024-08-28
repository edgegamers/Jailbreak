﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD;

public class AutoRTDListener(IRTDRewarder rewarder) : IPluginBehavior {
  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var player in Utilities.GetPlayers()
     .Where(player
        => AdminManager.PlayerHasPermissions(player, "@ego/dssilver"))
     .Where(player => !rewarder.HasReward(player)))
      player.ExecuteClientCommandFromServer("css_rtd");

    return HookResult.Continue;
  }
}