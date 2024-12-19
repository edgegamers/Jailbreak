using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Services;
using Jailbreak.English.RTD;
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

  private static readonly Queue<(PlayerWrapper, int)> rewards = new();

  public bool PrepareReward(CCSPlayerController player) {
    var eco = API.Gangs?.Services.GetService<IEcoManager>();
    if (eco == null) return false;
    var wrapper = new PlayerWrapper(player);

    rewards.Enqueue((wrapper, credits));

    Task.Run(async () => await processQueue());

    if (Math.Abs(credits) >= 5000)
      locale.JackpotReward(player, credits).ToAllChat();

    return true;
  }

  private async Task processQueue() {
    while (true) {
      var eco = API.Gangs?.Services.GetService<IEcoManager>();
      if (eco == null) {
        rewards.Clear();
        return;
      }

      if (rewards.Count == 0) return;
      var (player, amount) = rewards.Dequeue();

      await eco.Grant(player, amount, true, "RTD");
    }
  }

  public bool GrantReward(CCSPlayerController player) { return true; }
}