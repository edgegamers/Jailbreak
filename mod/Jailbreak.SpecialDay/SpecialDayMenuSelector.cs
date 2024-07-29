using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay;

public class SpecialDayMenuSelector {
  private readonly Func<SDType, string> command;
  private readonly CenterHtmlMenu menu;

  public SpecialDayMenuSelector(ISpecialDayFactory factory, BasePlugin plugin) :
    this(factory, sd => "css_sd " + (int)sd, plugin) { }

  public SpecialDayMenuSelector(ISpecialDayFactory factory,
    Func<SDType, string> command, BasePlugin plugin) {
    this.command = command;
    menu         = new CenterHtmlMenu("css_sd [SD]", plugin);
    foreach (SDType sd in Enum.GetValues(typeof(SDType))) {
      if (!factory.IsValidType(sd)) continue;
      var inst = factory.CreateSpecialDay(sd);
      menu.AddMenuOption(inst.Messages.Name, (p, _) => OnSelectLR(p, sd));
    }
  }

  public CenterHtmlMenu GetMenu() { return menu; }

  private void OnSelectLR(CCSPlayerController player, SDType sd) {
    MenuManager.CloseActiveMenu(player);
    player.ExecuteClientCommandFromServer(command.Invoke(sd));
  }
}