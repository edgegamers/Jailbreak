using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.SpecialDay;
using MySqlConnector;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Zones;

public class SqlZoneManager(IZoneFactory factory) : IZoneManager {
  public static FakeConVar<string> CvSqlTable = new("css_jb_zonetable",
    "cs2_jb_zones", "The table name for the zones");

  public static FakeConVar<string> CvSqlConnectionString =
    new("css_jb_sqlconnection", "Database=jailbreak;Host=localhost",
      "The connection string for the database", ConVarFlags.FCVAR_PROTECTED);

  private readonly IDictionary<ZoneType, IList<IZone>> zones =
    new Dictionary<ZoneType, IList<IZone>>();

  private BasePlugin plugin = null!;
  private IZoneFactory zoneFactory = null!;

  public void Start(BasePlugin basePlugin) {
    plugin      = basePlugin;
    zoneFactory = factory;

    Server.NextFrameAsync(createTable);

    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
  }

  public void Dispose() {
    plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }

  public async void LoadZones(string map) {
    var tasks = Enum.GetValues<ZoneType>()
     .Select(type => LoadZones(map, type))
     .ToList();

    await Task.WhenAll(tasks);
  }

  public Task<IList<IZone>> GetZones(string map, ZoneType type) {
    return Task.FromResult(!zones.TryGetValue(type, out var result) ?
      new List<IZone>() :
      result);
  }

  public Task<IDictionary<ZoneType, IList<IZone>>> GetAllZones(string map) {
    return Task.FromResult(zones);
  }

  private async void createTable() {
    var conn = new MySqlConnection(CvSqlConnectionString.Value);
    await conn.OpenAsync();

    var cmdText = $"""
            CREATE TABLE IF NOT EXISTS {CvSqlTable.Value} (
              zoneid INT NOT NULL AUTO_INCREMENT,
              pointid INT NOT NULL AUTO_INCREMENT,
              map VARCHAR(64) NOT NULL,
              type VARCHAR(32) NOT NULL,
              X FLOAT NOT NULL,
              Y FLOAT NOT NULL,
              Z FLOAT NOT NULL,
              PRIMARY KEY (id, map, type)
            )
      """;

    var cmd = new MySqlCommand(cmdText, conn);
    await cmd.ExecuteNonQueryAsync();
  }

  private void OnMapStart(string mapname) {
    Server.NextFrameAsync(() => LoadZones(mapname));
  }

  private MySqlCommand queryAllZones(string map) {
    var cmdText = $"""
          SELECT * FROM {CvSqlTable.Value}
          WHERE map = '{map}'
          ORDER BY zoneid, pointid DESC
      """;

    return new MySqlCommand(cmdText);
  }

  public async Task LoadZones(string map, ZoneType type) {
    var conn = new MySqlConnection(CvSqlConnectionString.Value);

    var cmd = queryAllZones(map);

    await conn.OpenAsync();
    cmd.Connection = conn;

    var reader = await cmd.ExecuteReaderAsync();

    var points      = new List<Vector>();
    var currentZone = 0;
    var pointId     = 0;
    while (await reader.ReadAsync()) {
      var point = new Vector(reader.GetFloat("X"), reader.GetFloat("Y"),
        reader.GetFloat("Z"));
      points.Add(point);
      var zoneId = reader.GetInt32("zoneid");
      pointId = reader.GetInt32("pointid");
      if (pointId == 0 || zoneId != currentZone) {
        if (pointId == 0 != (zoneId != currentZone))
          printNotClosedWarning(map, zoneId, pointId, currentZone);
        // Assume the zone is closed and allow the new zone to be created
        var zone = zoneFactory.CreateZone(points);
        if (!zones.ContainsKey(type)) zones[type] = new List<IZone>();
        zones[type].Add(zone);
      }
    }

    if (pointId != 0) printNotClosedWarning(map, -1, pointId, currentZone);
  }

  private void printNotClosedWarning(string map, int zoneId, int pointId,
    int currentZone) {
    if (zoneId == -1) {
      Server.PrintToConsole(
        "[WARNING] Expected a closing point but read end of table");
      Server.PrintToConsole(
        $"{map} Read: {zoneId} {pointId} Current Zone: {currentZone}");
      return;
    }

    Server.PrintToConsole("[WARNING] Encountered a new zone introduction but "
      + "the previous zone was not closed properly");
    Server.PrintToConsole(
      $"{map} Read: {zoneId} {pointId} Current Zone: {currentZone}");
  }
}