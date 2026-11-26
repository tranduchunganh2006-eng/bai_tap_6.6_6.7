using AtmBanking.DTO;

namespace AtmBanking.DAL;

/// <summary>
/// Gia lap viec tai & doi chieu hang tram nghin giao dich tu may chu mat 4-6 giay.
/// </summary>
public class ReconciliationDAL
{
    private const int TotalTransactions = 250_000;
    private const int Steps = 20;

    public async Task<ReconcileResultDTO> LoadAndCompareAsync(IProgress<int> progress, CancellationToken ct)
    {
        var started = DateTime.Now;
        int totalMs = Random.Shared.Next(4000, 6001);   // 4 - 6 giay
        int stepMs = totalMs / Steps;
        int mismatch = 0;

        for (int i = 1; i <= Steps; i++)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(stepMs, ct);                // gia lap cho phan hoi may chu
            mismatch += Random.Shared.Next(0, 40);        // gia lap so giao dich lech o moi lo
            progress?.Report(i * 100 / Steps);
        }

        return new ReconcileResultDTO
        {
            TongGiaoDich = TotalTransactions,
            LechLenh = mismatch,
            KhopLenh = TotalTransactions - mismatch,
            ThoiGian = DateTime.Now - started
        };
    }
}
