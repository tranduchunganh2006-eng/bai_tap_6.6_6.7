using AtmBanking.BUS;

namespace AtmBanking.GUI;

/// <summary>Form con 2: đối soát bất đồng bộ (async/await + CancellationTokenSource).</summary>
public class frmAtmReconciliation : Form
{
    private readonly ReconciliationBUS _bus = new();
    private readonly Button btnChay = new() { Left = 20, Top = 20, Width = 170, Height = 34, Text = "Chạy Đối Soát Tự Động" };
    private readonly Button btnDung = new() { Left = 200, Top = 20, Width = 130, Height = 34, Text = "Dừng Đối Soát", Enabled = false };
    private readonly ProgressBar prb = new() { Left = 20, Top = 70, Width = 520, Height = 22 };
    private readonly Label lblPercent = new() { Left = 550, Top = 74, AutoSize = true, Text = "0%" };
    private readonly Label lblClock = new() { Left = 20, Top = 100, AutoSize = true, ForeColor = Color.DarkBlue };
    private readonly TextBox txtLog = new() { Left = 20, Top = 130, Width = 560, Height = 190, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private readonly System.Windows.Forms.Timer clock = new() { Interval = 200 };
    private CancellationTokenSource _cts;
    private bool _running;

    private frmBankAtmMain Main => MdiParent as frmBankAtmMain;

    public frmAtmReconciliation()
    {
        Text = "Đối soát Giao dịch Cuối ngày";
        ClientSize = new Size(610, 340);
        Controls.AddRange(new Control[] { btnChay, btnDung, prb, lblPercent, lblClock, txtLog });

        btnChay.Click += BtnChay_Click;
        btnDung.Click += (_, _) => _cts?.Cancel();
        // Đồng hồ chạy liên tục chứng minh UI Thread KHÔNG bị treo khi đang đối soát
        clock.Tick += (_, _) => lblClock.Text = "Đồng hồ UI (chứng minh không treo): " + DateTime.Now.ToString("HH:mm:ss.fff");
        clock.Start();
        FormClosing += (_, _) => { _cts?.Cancel(); clock.Stop(); };
    }

    private async void BtnChay_Click(object sender, EventArgs e)
    {
        _cts = new CancellationTokenSource();
        SetRunning(true);
        Log("Bắt đầu tải & đối chiếu giao dịch rút tiền/chuyển khoản từ máy chủ...");

        var progress = new Progress<int>(p =>
        {
            if (!_running || IsDisposed) return;
            prb.Value = p;
            lblPercent.Text = p + "%";
            Main?.SetProgress(p);                      // cập nhật ToolStripProgressBar trên Form cha
        });

        try
        {
            var r = await _bus.ReconcileAsync(progress, _cts.Token);
            Log($"HOÀN TẤT sau {r.ThoiGian.TotalSeconds:0.0}s: tổng {r.TongGiaoDich:N0} GD, " +
                $"khớp {r.KhopLenh:N0}, lệch {r.LechLenh:N0}.");
        }
        catch (OperationCanceledException)
        {
            Log("Đã DỪNG đối soát theo yêu cầu người dùng.");
        }
        catch (Exception ex)
        {
            Log("LỖI: " + ex.Message);
        }
        finally
        {
            _cts.Dispose(); _cts = null;
            if (!IsDisposed)
            {
                SetRunning(false);
                prb.Value = 0; lblPercent.Text = "0%";
            }
            Main?.SetProgress(0);
        }
    }

    private void SetRunning(bool running)
    {
        _running = running;
        btnChay.Enabled = !running;
        btnDung.Enabled = running;
    }

    private void Log(string msg) => txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
}
