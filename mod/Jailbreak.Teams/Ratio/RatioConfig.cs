namespace Jailbreak.Teams.Ratio;

public class RatioConfig
{
	/// <summary>
	/// The minimum amount of Ts per every CT.
	/// When this is passed, CTs will be added until Target is reached.
	/// </summary>
	public double Minimum { get; set; } = 2.5;

	/// <summary>
	/// The maximum amount of Ts per every CT.
	/// When this is passed, CTs will be removed until Target is reached.
	/// </summary>
	public double Maximum { get; set; } = 4;

	/// <summary>
	/// When the ratio is autobalanced, the amount of guards
	/// should be total_players / target.
	/// </summary>
	public double Target { get; set; } = 4;
}
