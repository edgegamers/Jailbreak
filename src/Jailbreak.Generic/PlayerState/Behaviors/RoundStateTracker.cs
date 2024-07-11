﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Generic.PlayerState.Behaviors;

public class RoundStateTracker : BaseStateTracker, IPluginBehavior {
  public void Start(BasePlugin basePlugin) { }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info) {
    ResetAll();

    return HookResult.Continue;
  }
}