using Microsoft.Data.SqlClient;
using System.Data;

namespace AtmBanking.DAL;

public static class DbHelper
{
    // Doi chuoi ket noi neu dung SQL Server khac LocalDB
    public static string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=AtmDb;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5;";

    public static DataTable ExecuteQuery(string sql, params SqlParameter[] ps)
    {
        using var conn = new SqlConnection(ConnectionString);
        using var cmd = new SqlCommand(sql, conn);
        if (ps != null) cmd.Parameters.AddRange(ps);
        var dt = new DataTable();
        using var da = new SqlDataAdapter(cmd);
        da.Fill(dt);
        return dt;
    }

    public static int ExecuteNonQuery(string sql, params SqlParameter[] ps)
    {
        using var conn = new SqlConnection(ConnectionString);
        using var cmd = new SqlCommand(sql, conn);
        if (ps != null) cmd.Parameters.AddRange(ps);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public static object ExecuteScalar(string sql, params SqlParameter[] ps)
    {
        using var conn = new SqlConnection(ConnectionString);
        using var cmd = new SqlCommand(sql, conn);
        if (ps != null) cmd.Parameters.AddRange(ps);
        conn.Open();
        return cmd.ExecuteScalar();
    }

    public static bool TestConnection()
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return true;
        }
        catch { return false; }
    }
}
