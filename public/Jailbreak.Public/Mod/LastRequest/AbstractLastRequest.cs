using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest
{
   protected CCSPlayerController prisoner, guard;
   protected LRType type;
   protected LRState state;
   
   protected DateTimeOffset startTime;
   
   public abstract void Setup();
   public abstract void Execute();
   public abstract void End(LRResult result);
}