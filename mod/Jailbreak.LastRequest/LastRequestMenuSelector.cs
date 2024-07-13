using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestMenuSelector {
  private readonly Func<LRType, string> command;
  private readonly CenterHtmlMenu menu;

  public LastRequestMenuSelector(ILastRequestFactory factory, BasePlugin plugin)
    : this(factory, lr => "css_lr " + (int)lr, plugin) { }

  public LastRequestMenuSelector(ILastRequestFactory factory,
    Func<LRType, string> command, BasePlugin plugin) {
    this.command = command;
    menu         = new CenterHtmlMenu("css_lr [LR] [Player]", plugin);
    foreach (LRType lr in Enum.GetValues(typeof(LRType))) {
      if (!factory.IsValidType(lr)) continue;
      menu.AddMenuOption(lr.ToFriendlyString(), (p, _) => OnSelectLR(p, lr));
    }
  }

  public CenterHtmlMenu GetMenu() { return menu; }

  private void OnSelectLR(CCSPlayerController player, LRType lr) {
    MenuManager.CloseActiveMenu(player);
    player.ExecuteClientCommandFromServer(command.Invoke(lr));
  }
}