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
      SDType.FFA     => new FFASpecialDay(plugin, provider),
      SDType.WARDAY  => new WardaySpecialDay(plugin, provider),
      SDType.HNS     => new HideAndSeekDay(plugin, provider),
      SDType.NOSCOPE => new NoScopeDay(plugin, provider),
      _              => throw new NotImplementedException()
    };
  }

  public bool IsValidType(SDType type) {
    return type switch {
      SDType.FFA     => true,
      SDType.WARDAY  => true,
      SDType.HNS     => true,
      SDType.NOSCOPE => true,
      _              => false
    };
  }
}