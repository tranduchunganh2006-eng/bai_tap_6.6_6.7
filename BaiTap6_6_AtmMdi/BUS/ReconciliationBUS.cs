using AtmBanking.DAL;
using AtmBanking.DTO;

namespace AtmBanking.BUS;

public class ReconciliationBUS
{
    private readonly ReconciliationDAL _dal = new();

    /// <summary>
    /// Chay doi soat tren thread pool (Task.Run) de UI Thread khong bi treo.
    /// Ho tro huy qua CancellationToken.
    /// </summary>
    public Task<ReconcileResultDTO> ReconcileAsync(IProgress<int> progress, CancellationToken ct)
        => Task.Run(() => _dal.LoadAndCompareAsync(progress, ct), ct);
}
