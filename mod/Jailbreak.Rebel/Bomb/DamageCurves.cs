namespace Jailbreak.Rebel.Bomb;

public static class DamageCurves
{
	/// <summary>
	/// Implements a curve similar to the top right quadrant of a circle
	/// </summary>
	/// <param name="peak"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	public static float CircleUpper(float peak, float range, float distance)
	{
		if (distance >= range)
			return 0;

		float distSquared = distance * distance;
		float rangeSquared = range * range;
		float peakSquared = peak * peak;

		float xComponent = (distSquared / rangeSquared) - 1;
		float damage = MathF.Sqrt((-peakSquared) * xComponent);

		return Math.Max(0, damage);
	}

	/// <summary>
	/// Implements a curve similar to the bottom left quadrant of a circle
	/// </summary>
	/// <param name="peak"></param>
	/// <param name="range"></param>
	/// <param name="distance"></param>
	/// <returns></returns>
	public static float CircleLower(float peak, float range, float distance)
	{
		if (distance >= range)
			return 0;

		float offsetDist = distance - range;
		float distSquared = offsetDist * offsetDist;
		float rangeSquared = range * range;
		float peakSquared = peak * peak;

		float xComponent = (distSquared / rangeSquared) - 1;
		float damage = peak - MathF.Sqrt((-peakSquared) * xComponent);

		return Math.Max(0, damage);
	}
}
