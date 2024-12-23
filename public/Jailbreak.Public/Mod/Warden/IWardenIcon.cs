using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenIcon {
  void AssignWardenIcon(CCSPlayerController warden);
  void RemoveWardenIcon(CCSPlayerController warden);
}