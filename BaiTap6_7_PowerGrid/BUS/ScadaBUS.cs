using PowerGrid.DAL;
using PowerGrid.DTO;

namespace PowerGrid.BUS;

public class ScadaBUS
{
    public const double OverloadThreshold = 85.0;   // ngưỡng an toàn (%)
    public const int IntervalMs = 2000;             // 2 giây / lần

    private readonly TelemetryDAL _telemetry = new();
    private readonly SubstationDAL _substations = new();

    public List<string> GetStations() => _substations.GetNames();

    public TelemetryDTO Collect(string station)
    {
        var t = _telemetry.Read(station);
        t.IsOverload = t.LoadPercent > OverloadThreshold;
        if (t.IsOverload)
            _ = Task.Run(() => _substations.InsertOverloadLog(t.StationName, t.Time, t.VoltageKv, t.LoadPercent));
        return t;
    }
}
