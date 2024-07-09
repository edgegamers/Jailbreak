namespace Jailbreak.Public.Generic;

public interface ICoroutines
{
    /// <summary>
    ///     Invoke a coroutine within the current round.
    ///     Do not allow this coroutine to pass onto other rounds.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="time"></param>
    void Round(Action callback, float time = 10.0f);
}