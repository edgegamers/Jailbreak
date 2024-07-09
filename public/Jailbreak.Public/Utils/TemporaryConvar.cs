using CounterStrikeSharp.API.Modules.Cvars;

namespace Jailbreak.Public.Utils;

public class TemporaryConvar<T> : IDisposable {
  private readonly ConVar _handle;
  private readonly T _previousValue;

  public TemporaryConvar(string name, T value) {
    _handle = ConVar.Find(name) ?? throw new InvalidOperationException();
    if (_handle == null)
      throw new InvalidOperationException($"ConVar {name} does not exist!");

    _previousValue = _handle.GetPrimitiveValue<T>();
    _handle.SetValue(value);
  }

  public void Dispose() { _handle.SetValue(_previousValue); }
}