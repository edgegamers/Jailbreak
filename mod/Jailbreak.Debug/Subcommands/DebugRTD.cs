using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.RTD;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// ReSharper disable once InconsistentNaming
public class DebugRTD(IServiceProvider services, BasePlugin plugin)
  : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    var rewarder = Services.GetRequiredService<IRTDRewarder>();
    var rewards  = Services.GetRequiredService<IRewardGenerator>().ToList();

    var menu = new CenterHtmlMenu("Debug RTD", plugin);

    foreach (var reward in rewards) {
      menu.AddMenuOption($"{reward.Item1.Name} - Prob: {reward.Item2}",
        (p, _) => { rewarder.SetReward(p, reward.Item1); });
    }
  }
}