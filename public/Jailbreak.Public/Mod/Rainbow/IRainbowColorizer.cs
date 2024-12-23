using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Rainbow;

public interface IRainbowColorizer : IPluginBehavior {
  public static readonly string RAINBOW =
    $"{ChatColors.DarkRed}R{ChatColors.Orange}a{ChatColors.Yellow}i{ChatColors.Green}n{ChatColors.LightBlue}b{ChatColors.Blue}o{ChatColors.Purple}w{ChatColors.Grey}";

  void StartRainbow(CCSPlayerController player);

  void StopRainbow(CCSPlayerController player) {
    if (!player.IsValid) return;
    StopRainbow(player.Slot);
  }

  void StopRainbow(int slot);

  Color GetRainbow();
}