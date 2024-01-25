using CounterStrikeSharp.API.Modules.Cvars;

namespace Jailbreak.Public.Utils;

public class TemporaryConvar : IDisposable
{
	private string _previousValue;
	private ConVar _handle;
	public TemporaryConvar(string name, string value)
	{
		_handle = ConVar.Find(name);
		if (_handle == null)
			throw new InvalidOperationException($"ConVar {name} does not exist!");

		_previousValue = _handle.StringValue;
		_handle.SetValue(value);
	}

	public void Dispose()
	{
		_handle.StringValue = _previousValue;
	}
}
