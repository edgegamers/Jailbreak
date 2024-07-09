using CounterStrikeSharp.API.Modules.Cvars;

namespace Jailbreak.Public.Utils;

public class TemporaryConvar<T> : IDisposable {
  private readonly ConVar handle;
  private readonly T previousValue;

  public TemporaryConvar(string name, T value) {
    handle = ConVar.Find(name) ?? throw new InvalidOperationException();
    if (handle == null)
      throw new InvalidOperationException($"ConVar {name} does not exist!");

    previousValue = handle.GetPrimitiveValue<T>();
    handle.SetValue(value);
  }

  public void Dispose() { handle.SetValue(previousValue); }
}