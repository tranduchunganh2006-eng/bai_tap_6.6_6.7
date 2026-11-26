using AtmBanking.DTO;
using Microsoft.Data.SqlClient;

namespace AtmBanking.DAL;

public class AtmStationDAL
{
    public List<AtmStationDTO> GetAll()
    {
        var dt = DbHelper.ExecuteQuery(
            "SELECT MaTram, DiaChi, TonQuy, HoatDong, AnhPath FROM AtmStations ORDER BY MaTram");
        var list = new List<AtmStationDTO>();
        foreach (System.Data.DataRow r in dt.Rows)
        {
            list.Add(new AtmStationDTO
            {
                MaTram = r["MaTram"].ToString(),
                DiaChi = r["DiaChi"].ToString(),
                TonQuy = Convert.ToDecimal(r["TonQuy"]),
                HoatDong = Convert.ToBoolean(r["HoatDong"]),
                AnhPath = r["AnhPath"] == DBNull.Value ? null : r["AnhPath"].ToString()
            });
        }
        return list;
    }

    public bool Exists(string ma) =>
        Convert.ToInt32(DbHelper.ExecuteScalar(
            "SELECT COUNT(*) FROM AtmStations WHERE MaTram=@ma",
            new SqlParameter("@ma", ma))) > 0;

    public int Insert(AtmStationDTO s) => DbHelper.ExecuteNonQuery(
        "INSERT INTO AtmStations(MaTram, DiaChi, TonQuy, HoatDong, AnhPath) VALUES(@ma,@dc,@tq,@hd,@anh)",
        Params(s));

    public int Update(AtmStationDTO s) => DbHelper.ExecuteNonQuery(
        "UPDATE AtmStations SET DiaChi=@dc, TonQuy=@tq, HoatDong=@hd, AnhPath=@anh WHERE MaTram=@ma",
        Params(s));

    public int Delete(string ma) => DbHelper.ExecuteNonQuery(
        "DELETE FROM AtmStations WHERE MaTram=@ma", new SqlParameter("@ma", ma));

    private static SqlParameter[] Params(AtmStationDTO s) => new[]
    {
        new SqlParameter("@ma", s.MaTram),
        new SqlParameter("@dc", s.DiaChi),
        new SqlParameter("@tq", s.TonQuy),
        new SqlParameter("@hd", s.HoatDong),
        new SqlParameter("@anh", (object)s.AnhPath ?? DBNull.Value)
    };
}
