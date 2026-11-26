using AtmBanking.DAL;
using AtmBanking.DTO;

namespace AtmBanking.BUS;

public class AtmStationBUS
{
    private readonly AtmStationDAL _dal = new();

    public List<AtmStationDTO> GetAll() => _dal.GetAll();

    public Task<bool> CheckCoreBankingAsync() => Task.Run(DbHelper.TestConnection);

    public string Validate(AtmStationDTO s, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(s.MaTram)) return "Mã trạm không được để trống.";
        if (string.IsNullOrWhiteSpace(s.DiaChi)) return "Địa chỉ đặt máy không được để trống.";
        if (s.TonQuy < 0) return "Số tiền tồn quỹ không được âm.";
        if (isNew && _dal.Exists(s.MaTram.Trim())) return "Mã trạm đã tồn tại.";
        return null;
    }

    public string Add(AtmStationDTO s)
    {
        var err = Validate(s, true);
        if (err != null) return err;
        s.MaTram = s.MaTram.Trim();
        _dal.Insert(s);
        return null;
    }

    public string Update(AtmStationDTO s)
    {
        var err = Validate(s, false);
        if (err != null) return err;
        _dal.Update(s);
        return null;
    }

    public void Delete(string ma) => _dal.Delete(ma);
}
