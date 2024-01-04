using System.Reflection;

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

using Jailbreak.Public.Behaviors;

using Serilog;

namespace Jailbreak.Generic.Behaviors;

public class RoundStateTracker : BaseStateTracker, IPluginBehavior
{
	public void Start(BasePlugin parent)
	{

	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info)
	{
		Log.Debug("[RoundStateTracker] Resetting all tracked states");
		ResetAll();

		return HookResult.Continue;
	}
}
