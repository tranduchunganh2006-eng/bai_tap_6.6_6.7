using Microsoft.Data.SqlClient;

namespace PowerGrid.DAL;

public class SubstationDAL
{
    public static string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=PowerGridDb;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;";

    private static readonly string[] Defaults =
        { "Trạm 110kV Hà Đông", "Trạm 220kV Chèm", "Trạm 110kV Thành Công" };

    /// <summary>Lấy danh sách trạm từ DB; nếu chưa có CSDL thì dùng danh sách mặc định.</summary>
    public List<string> GetNames()
    {
        try
        {
            var list = new List<string>();
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("SELECT Name FROM Substations ORDER BY Id", conn);
            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(rd.GetString(0));
            return list.Count > 0 ? list : Defaults.ToList();
        }
        catch { return Defaults.ToList(); }
    }

    /// <summary>Ghi nhật ký quá tải xuống DB (bỏ qua lỗi nếu chưa cài CSDL).</summary>
    public void InsertOverloadLog(string station, DateTime at, double kv, double load)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(
                "INSERT INTO OverloadLogs(StationName, LoggedAt, VoltageKv, LoadPercent) VALUES(@s,@t,@v,@l)", conn);
            cmd.Parameters.AddWithValue("@s", station);
            cmd.Parameters.AddWithValue("@t", at);
            cmd.Parameters.AddWithValue("@v", kv);
            cmd.Parameters.AddWithValue("@l", load);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        catch { /* không để lỗi DB làm gián đoạn vòng lặp giám sát */ }
    }
}
