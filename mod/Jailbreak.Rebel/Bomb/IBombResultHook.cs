namespace Jailbreak.Rebel.Bomb;

public interface IBombResultHook
{
	/// <summary>
	/// Called after a player detonates the bomb
	/// </summary>
	/// <param name="result"></param>
	void OnDetonation(BombResult result);
}
