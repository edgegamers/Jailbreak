using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Rainbow;

public interface IRainbowColorizer : IPluginBehavior {
  void StartRainbow(CCSPlayerController player);

  void StopRainbow(CCSPlayerController player) {
    if (!player.IsValid) return;
    StopRainbow(player.Slot);
  }

  void StopRainbow(int slot);
}