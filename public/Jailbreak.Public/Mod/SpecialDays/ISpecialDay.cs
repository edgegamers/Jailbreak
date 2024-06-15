namespace Jailbreak.Public.Mod.SpecialDays;

public interface ISpecialDay
{
    string Name { get;  }
    string Description { get; }
    void OnStart();
    void OnEnd();
    bool ShouldJihadC4BeEnabled { get; }
}