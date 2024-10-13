using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class ChatSpyReward : IRTDReward {
  public ChatSpyReward(BasePlugin plugin) {
    plugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd);
  }

  public string Name => "Chat Spy";

  public string Description
    => "You will be able to read all team chats next round.";

  public bool Enabled => API.Actain != null;

  public bool GrantReward(CCSPlayerController player) {
    if (API.Actain == null) return false;
    API.Actain.getSpyService().SetSpy(player.SteamID, true);
    return true;
  }

  private HookResult onRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var player in Utilities.GetPlayers()) {
      if (API.Actain == null) return HookResult.Continue;
      API.Actain.getSpyService().SetSpy(player.SteamID, false);
    }

    return HookResult.Continue;
  }
}