using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD;

public class RTDStatsCommand(IRewardGenerator generator) : IPluginBehavior {
  [ConsoleCommand("css_rtdstats", "View stats and probabilities of the die")]
  public void
    Command_RTDStats(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;
    var total = generator.Sum(r => r.Item2);

    var rewards = generator.ToList();
    rewards.Sort((a, b) => a.Item2.CompareTo(b.Item2));

    var index = 0;
    foreach (var (reward, prob) in rewards) {
      var name    = reward.Name;
      var percent = prob / total * 100;
      executor.PrintToChat(
        $"{ChatColors.Orange}{index++}. {ChatColors.LightBlue}{name}{ChatColors.Grey}: {ChatColors.Yellow}{percent:0.00}%");
      executor.PrintToConsole($"{index++}. {name}: {percent:0.00}%");
    }
  }
}