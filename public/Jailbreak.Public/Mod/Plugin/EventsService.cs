using System.Diagnostics.Tracing;

namespace Jailbreak.Public.Mod.Plugin;

public class EventsService : IEventsService
{
    // key: event name, value: callback
    private readonly Dictionary<string, Func<bool>> _eventListeners;

    public EventsService()
    {
        _eventListeners = new Dictionary<string, Func<bool>>();
    }

    public void RegisterEventListener(string eventName, Func<bool> eventListener)
    {
        _eventListeners.Add(eventName, eventListener);
    }

    public void FireEvent(string eventName)
    {
        foreach (var listener in _eventListeners)
        {
            if (listener.Key.Equals(eventName)) 
            {
                listener.Value.Invoke();
            }
        }
    }
}
