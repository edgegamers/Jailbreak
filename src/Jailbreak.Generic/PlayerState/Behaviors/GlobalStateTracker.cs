using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

using Jailbreak.Public.Behaviors;

namespace Jailbreak.Generic.PlayerState.Behaviors;

public class GlobalStateTracker : BaseStateTracker, IPluginBehavior
{
	public void Start(BasePlugin parent)
	{
	}

	/// <summary>
	///     Disconnect handler to reset states on user leave
	/// </summary>
	/// <param name="ev"></param>
	/// <param name="info"></param>
	[GameEventHandler]
	public HookResult OnDisconnect(EventPlayerDisconnect ev, GameEventInfo info)
	{
		Reset(ev.Userid);
		return HookResult.Continue;
	}
}