namespace Jailbreak.Public.Generic;

public interface IPlayerStateFactory
{
	/// <summary>
	///     This state lasts from when the player connect to until the player disconnects.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	IPlayerState<T> Global<T>()
		where T : class, new();

	/// <summary>
	///     This state resets when the player dies
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	IPlayerState<T> Alive<T>()
		where T : class, new();

	/// <summary>
	///     This state resets when the round ends or begins.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	IPlayerState<T> Round<T>()
		where T : class, new();
}