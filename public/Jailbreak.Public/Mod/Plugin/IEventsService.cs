
namespace Jailbreak.Public.Mod.Plugin;

// Ideally very simple event driven callback service...
// Allows parts of the plugin to tell each other it's done things.
// this must be registered before any other service...
public interface IEventsService
{

    // usage: CreateEventListener( bool (eventName) => { callback here } )
    // returns true if successful
    void RegisterEventListener(string eventName, Func<bool> eventListener);

    void FireEvent(string eventName);

}
