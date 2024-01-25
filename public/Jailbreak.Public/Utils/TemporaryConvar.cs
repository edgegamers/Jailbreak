using CounterStrikeSharp.API.Modules.Cvars;

namespace Jailbreak.Public.Utils;

public class TemporaryConvar<T> : IDisposable
{
	private T _previousValue;
	private ConVar _handle;
	public TemporaryConvar(string name, T value)
	{
		_handle = ConVar.Find(name);
		if (_handle == null)
			throw new InvalidOperationException($"ConVar {name} does not exist!");

		_previousValue = _handle.GetPrimitiveValue<T>();
		_handle.SetValue<T>(value);
	}

	public void Dispose()
	{
		_handle.SetValue<T>(_previousValue);
	}
}
