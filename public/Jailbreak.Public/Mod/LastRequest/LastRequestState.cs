namespace Jailbreak.Public.Mod.LastRequest;

public enum LastRequestState
{
    BEGINNING, // Last Request has been announced, players might be frozen or waiting for guns
    PLAYING, // Last Request is currently in progress, players should be actively playing (throwing he nades, knife
             // fights, etc)
    RUSHING, // Last Request is taking too long, we need to speed things up
    ENDED    // Last Request has ended, one of the players should be dead
}