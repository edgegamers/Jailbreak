using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestService
{
   bool IsLastRequestActive();
   bool SetLastRequestActive(bool lastRequest);

   bool IsInLastRequest(CCSPlayerController player)
   {
      return GetLastRequest(player) == null;
   }

   AbstractLastRequest? GetLastRequest(CCSPlayerController player);
   bool SetLastRequest(CCSPlayerController player, AbstractLastRequest lastRequest);

}