using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_markst [player]
public class MarkST(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (info.ArgCount == 1) {
      info.ReplyToCommand("Specify target?");
      return;
    }

    var target = GetVulnerableTarget(info);
    if (target == null) return;

    var stService = Services.GetRequiredService<ISpecialTreatmentService>();
    foreach (var player in target.Players)
      stService.SetSpecialTreatment(player,
        !stService.IsSpecialTreatment(player));

    info.ReplyToCommand("Toggled special treatment for " + GetTargetLabel(info)
      + ".");
  }
}