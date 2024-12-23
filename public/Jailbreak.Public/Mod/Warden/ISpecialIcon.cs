using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface ISpecialIcon {
  void AssignSpecialIcon(CCSPlayerController player);
  void RemoveSpecialIcon(CCSPlayerController player);
}