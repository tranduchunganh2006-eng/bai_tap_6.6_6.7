namespace PowerGrid.DTO;

public class TelemetryDTO
{
    public string StationName { get; set; }
    public DateTime Time { get; set; }
    public double VoltageKv { get; set; }      // 108 - 115 kV
    public double LoadPercent { get; set; }    // 50 - 98 %
    public bool IsOverload { get; set; }       // do tầng BUS xác định (> 85%)
}
