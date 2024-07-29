using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class FFAMessages : ISpecialDayMessages {
  public IView SpecialDayStart => new SimpleView { "Free For All has begun!" };
  public IView SpecialDayEnd => new SimpleView { "Free For All has ended!" };
}