namespace Jailbreak.Public.Mod.SpecialDays;

public interface ISpecialDayHandler
{
    int RoundsSinceLastSpecialDay();
    bool CanStartSpecialDay();
    bool IsSpecialDayActive();
    bool StartSpecialDay<T>(string name,T a);
}