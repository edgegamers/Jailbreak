using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.SpecialDay.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayFactory(IServiceProvider provider) : ISpecialDayFactory {
  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin) { plugin = basePlugin; }

  public AbstractSpecialDay CreateSpecialDay(SDType type) {
    return type switch {
      SDType.FFA       => new FFADay(plugin, provider),
      SDType.WARDAY    => new WardayDay(plugin, provider),
      SDType.HNS       => new HideAndSeekDay(plugin, provider),
      SDType.NOSCOPE   => new NoScopeDay(plugin, provider),
      SDType.INFECTION => new InfectionDay(plugin, provider),
      SDType.CUSTOM    => new CustomDay(plugin, provider),
      SDType.SPEEDRUN  => new SpeedrunDay(plugin, provider),
      SDType.OITC      => new OneInTheChamberDay(plugin, provider),
      SDType.TELEPORT  => new TeleportDay(plugin, provider),
      _                => throw new NotImplementedException()
    };
  }

  public bool IsValidType(SDType type) {
    try {
      CreateSpecialDay(type);
      return true;
    } catch (NotImplementedException) { return false; }
  }
}