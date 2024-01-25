using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Teams;
using Jailbreak.Public.Utils;

using Microsoft.Extensions.Logging;

using Serilog;

namespace Jailbreak.Teams.Ratio;

public class RatioBehavior : IPluginBehavior
{
	private IGuardQueue _guardQueue;
	private ILogger<RatioBehavior> _logger;

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

	public RatioBehavior(RatioConfig config, IGuardQueue guardQueue, ILogger<RatioBehavior> logger)
	{
		_config = config;
		_guardQueue = guardQueue;
		_logger = logger;
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

		_logger.LogTrace("[Ratio] Evaluating ratio of {@Ct}ct to {@T}t: {@TsPerCt}t/ct ratio, {@Target} target.", ct ,t ,ts_per_ct, target);

		if (_config.Maximum <= ts_per_ct)
		{
			//	There are too many Ts per CT!
			//	Get more guards on the team
			_logger.LogTrace("[Ratio] Decision: Not enough CTs: {@Maximum} <= {@TsPerCt}", _config.Maximum, ts_per_ct);
			return new(target - ct, RatioActionType.Add);
		}

		if (ts_per_ct <= _config.Minimum)
		{
			//	There are too many guards per T!
			_logger.LogTrace("[Ratio] Decision: Too many CTs: {@TsPerCt} <= {@Minimum}", ts_per_ct, _config.Minimum);
			return new(ct - target, RatioActionType.Remove);
		}

		_logger.LogTrace("[Ratio] Decision: Goldilocks: {@Maximum} (max) <= {@TsPerCt} (t/ct) <= {@Minimum} (min)", _config.Maximum, ts_per_ct, _config.Minimum);
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

		//	Prevent the round from ending while we make ratio adjustments
		using (var _ = new TemporaryConvar<bool>("mp_ignore_round_win_conditions", true))
		{
			var success = action.Type switch
			{
				RatioActionType.Add => _guardQueue.TryPop(action.Count),
				RatioActionType.Remove => _guardQueue.TryPush(action.Count),
				_ => true
			};

			if (!success)
				Server.PrintToChatAll($"[BUG] Ratio enforcement failed :^( [RatioAction: {action.Type} @ {action.Count}]");
		}

		return HookResult.Continue;
	}

	public void Dispose()
	{

	}
}
