using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs.Tags;

public class PlayerTagHelper(IServiceProvider provider) : IRichPlayerTag {
  private readonly Lazy<IRebelService> rebelService =
    new(provider.GetRequiredService<IRebelService>);

  private readonly Lazy<ISpecialTreatmentService> stService =
    new(provider.GetRequiredService<ISpecialTreatmentService>);

  private readonly Lazy<IWardenService> wardenService =
    new(provider.GetRequiredService<IWardenService>);

  //  Lazy-load dependencies to avoid loops, since we are a lower-level class.

  public FormatObject Rich(CCSPlayerController player) {
    if (wardenService.Value.IsWarden(player))
      return new StringFormatObject("(WARDEN)", ChatColors.DarkBlue);
    if (player.GetTeam() == CsTeam.CounterTerrorist)
      return new StringFormatObject("(CT)", ChatColors.BlueGrey);
    if (rebelService.Value.IsRebel(player))
      return new StringFormatObject("(REBEL)", ChatColors.DarkRed);
    if (stService.Value.IsSpecialTreatment(player))
      return new StringFormatObject("(ST)", ChatColors.Green);

    return new StringFormatObject("(T)", ChatColors.Yellow);
  }

  public string Plain(CCSPlayerController playerController) {
    return Rich(playerController).ToPlain();
  }
}