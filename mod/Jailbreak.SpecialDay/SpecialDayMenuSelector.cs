using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.SpecialDay.SpecialDays;

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
      var name = inst.Type.ToString();
      if (inst is ISpecialDayMessageProvider messaged)
        name = messaged.Messages.Name;
      menu.AddMenuOption(name, (p, _) => OnSelectSD(p, sd));
    }
  }

  public CenterHtmlMenu GetMenu() { return menu; }

  private void OnSelectSD(CCSPlayerController player, SDType sd) {
    MenuManager.CloseActiveMenu(player);
    player.ExecuteClientCommandFromServer(command.Invoke(sd));
  }
}