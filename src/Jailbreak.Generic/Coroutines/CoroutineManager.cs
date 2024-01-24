using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Generic;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Generic.Coroutines;

public class CoroutineManager : ICoroutines, IPluginBehavior
{
	private List<Timer> _destroyOnRoundEnd = new();

	private Timer New(Action callback, float time = 10)
		=> new(time, callback, TimerFlags.STOP_ON_MAPCHANGE);

	public void Round(Action callback, float time = 10)
	{
		var timer = New(callback, time);
		_destroyOnRoundEnd.Add(timer);
	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info)
	{
		_destroyOnRoundEnd.ForEach(timer => timer.Kill());

		return HookResult.Continue;
	}
}
