using PowerGrid.DTO;

namespace PowerGrid.DAL;

/// <summary>Giả lập bộ thu thập SCADA: sinh ngẫu nhiên thông số vận hành.</summary>
public class TelemetryDAL
{
    public TelemetryDTO Read(string stationName) => new()
    {
        StationName = stationName,
        Time = DateTime.Now,
        VoltageKv = Math.Round(108 + Random.Shared.NextDouble() * 7, 2),   // 108 - 115 kV
        LoadPercent = Math.Round(50 + Random.Shared.NextDouble() * 48, 1)  // 50 - 98 %
    };
}
