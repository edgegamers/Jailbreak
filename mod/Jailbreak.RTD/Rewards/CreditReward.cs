﻿using CounterStrikeSharp.API.Core;
using GangsAPI.Data;
using GangsAPI.Services;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.RTD;
using Jailbreak.Public;
using Jailbreak.Public.Mod.RTD;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.RTD.Rewards;

public class CreditReward(int credits, IRTDLocale locale) : IRTDReward {
  public string Name => $"{credits} credit{(credits == 1 ? "" : "s")}";

  public string Description => $"You won {Name}{(credits > 500 ? "!" : ".")}";

  public bool Enabled => API.Gangs != null;

  public bool PrepareReward(CCSPlayerController player) {
    var eco = API.Gangs?.Services.GetService<IEcoManager>();
    if (eco == null) return false;
    var wrapper = new PlayerWrapper(player);

    if (Math.Abs(credits) >= 5000)
      locale.JackpotReward(player, credits).ToAllChat();

    Task.Run(async () => await eco.Grant(wrapper, credits, true, "RTD"));
    return true;
  }

  public bool GrantReward(CCSPlayerController player) { return true; }
}