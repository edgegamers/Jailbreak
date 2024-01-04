using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Teams;

namespace Jailbreak.Teams.Ratio;

public class RatioBehavior : IPluginBehavior
{
	private IGuardQueue _guardQueue;

	public enum RatioActionType
	{
		Add,
		Remove,
		None,
	}

	public struct RatioAction
	{
		public RatioActionType Type { get; init; }
		public int Count { get; init; }

		public RatioAction(int count = 0, RatioActionType type = RatioActionType.None)
		{
			Count = count;
			Type = type;
		}
	}

	private RatioConfig _config;

	public RatioBehavior(RatioConfig config, IGuardQueue guardQueue)
	{
		_config = config;
		_guardQueue = guardQueue;
	}

	/// <summary>
	/// Evaluate whether the provided CT/T ratio needs
	/// </summary>
	/// <param name="ct"></param>
	/// <param name="t"></param>
	/// <returns></returns>
	public RatioAction Evaluate(int ct, int t)
	{
		//	No divide by zero errors today...
		//	Make value 0.01 so the decision will always be to add Ts
		//	1 / 0.01 = 100, etc...
		var normalized_ct = (ct == 0 ? 0.01 : (double)ct);
		double ts_per_ct = t / (double)normalized_ct;
		int target = (int)( (ct + t) / _config.Target ) + 1;

		Server.PrintToConsole($"[Ratio] Evaluating ratio of {ct}ct to {t}t: {ts_per_ct}t/ct ratio, {target} target.");

		if (_config.Maximum <= ts_per_ct)
		{
			//	There are too many Ts per CT!
			//	Get more guards on the team
			Server.PrintToConsole($"[Ratio] Decision: Not enough CTs: {_config.Maximum} <= {ts_per_ct}");
			return new(target - ct, RatioActionType.Add);
		}

		if (ts_per_ct <= _config.Minimum)
		{
			//	There are too many guards per T!
			Server.PrintToConsole($"[Ratio] Decision: Too many CTs: {ts_per_ct} <= {_config.Minimum}");
			return new(ct - target, RatioActionType.Remove);
		}

		Server.PrintToConsole($"[Ratio] Decision: Goldilocks: {_config.Maximum} (max) <= {ts_per_ct} (t/ct) <= {_config.Minimum} (min)");
		//	Take no action
		return new();
	}

	/// <summary>
	/// When a round starts, balance out the teams
	/// </summary>
	[GameEventHandler(HookMode.Pre)]
	public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
	{
		var counterTerrorists = Utilities.GetPlayers().Count(player => player.GetTeam() == CsTeam.CounterTerrorist);
		var terrorists = Utilities.GetPlayers().Count(player => player.GetTeam() == CsTeam.Terrorist);

		var action = Evaluate(counterTerrorists, terrorists);

		var success = action.Type switch
		{
			RatioActionType.Add => _guardQueue.TryPop(action.Count),
			RatioActionType.Remove => _guardQueue.TryPush(action.Count),
			_ => true
		};

		if (!success)
			Server.PrintToChatAll($"[BUG] Ratio enforcement failed :^( [RatioAction: {action.Type} @ {action.Count}]");

		return HookResult.Continue;
	}

	public void Dispose()
	{

	}
}
