namespace Jailbreak.Public.Mod.SpecialDays;

public interface ISpecialDayHandler
{
    int RoundsSinceLastSpecialDay();
    bool CanStartSpecialDay();
    bool IsSpecialDayActive();
    bool StartSpecialDay(string name);
}