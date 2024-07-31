using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Debug.Subcommands;

public interface ITypedZoneCreator : IZoneCreator {
  ZoneType Type { get; }
}