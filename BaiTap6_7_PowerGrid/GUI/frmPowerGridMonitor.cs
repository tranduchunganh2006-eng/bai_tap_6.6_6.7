using PowerGrid.BUS;
using PowerGrid.DTO;

namespace PowerGrid.GUI;

/// <summary>Trung tâm điều độ & giám sát lưới điện - Thread Safety trong WinForms.</summary>
public class frmPowerGridMonitor : Form
{
    private readonly ScadaBUS _bus = new();

    // Khu vực điều khiển
    private readonly Button btnStart = new() { Text = "Bắt đầu Giám sát SCADA", Left = 20, Top = 25, Width = 180, Height = 32 };
    private readonly Button btnStop = new() { Text = "Ngắt Kết nối", Left = 210, Top = 25, Width = 120, Height = 32, Enabled = false };
    private readonly ComboBox cboStation = new() { Left = 345, Top = 29, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };

    // Bảng thông số trực quan
    private readonly Label lblVoltage = new() { Name = "lblVoltage", Left = 20, Top = 30, AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), Text = "Điện áp: -- kV" };
    private readonly Label lblLoadPercent = new() { Name = "lblLoadPercent", Left = 20, Top = 80, AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), Text = "Mức mang tải: -- %" };
    private readonly Label lblAlert = new() { Left = 340, Top = 85, AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Red, Text = "" };

    // Nhật ký sự cố
    private readonly DataGridView dgvGridLogs = new() { Name = "dgvGridLogs", Dock = DockStyle.Fill };

    // StatusStrip
    private readonly StatusStrip statusStrip = new();
    private readonly ToolStripStatusLabel lblLastTime = new() { Text = "Gói tin cuối: --:--:--" };
    private readonly ToolStripStatusLabel lblCycles = new() { Text = "Tổng số chu kỳ đã quét: 0", Spring = true, TextAlign = ContentAlignment.MiddleRight };

    private CancellationTokenSource _cts;

    public frmPowerGridMonitor()
    {
        Text = "Trung tâm điều độ & Giám sát lưới điện thời gian thực (SCADA)";
        Size = new Size(820, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var grpControl = new GroupBox { Text = "Khu vực điều khiển", Dock = DockStyle.Top, Height = 75 };
        grpControl.Controls.AddRange(new Control[] { btnStart, btnStop, cboStation });

        var grpDash = new GroupBox { Text = "Bảng thông số trực quan (Real-time Dashboard)", Dock = DockStyle.Top, Height = 140 };
        grpDash.Controls.AddRange(new Control[] { lblVoltage, lblLoadPercent, lblAlert });

        var grpLog = new GroupBox { Text = "Nhật ký sự cố - trạm biến áp rơi vào trạng thái Quá tải", Dock = DockStyle.Fill };
        // DataGridView cấu hình thủ công bằng code
        dgvGridLogs.AllowUserToAddRows = false;
        dgvGridLogs.ReadOnly = true;
        dgvGridLogs.RowHeadersVisible = false;
        dgvGridLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvGridLogs.Columns.Add("colTime", "Thời gian");
        dgvGridLogs.Columns.Add("colStation", "Trạm biến áp");
        dgvGridLogs.Columns.Add("colVoltage", "Điện áp (kV)");
        dgvGridLogs.Columns.Add("colLoad", "Mức tải (%)");
        dgvGridLogs.Columns.Add("colMsg", "Cảnh báo");
        grpLog.Controls.Add(dgvGridLogs);

        statusStrip.Items.AddRange(new ToolStripItem[] { lblLastTime, lblCycles });

        // Thứ tự Add quyết định Dock: Fill trước, Top sau (add sau nằm trên cùng)
        Controls.Add(grpLog);
        Controls.Add(grpDash);
        Controls.Add(grpControl);
        Controls.Add(statusStrip);

        cboStation.Items.AddRange(_bus.GetStations().Cast<object>().ToArray());
        if (cboStation.Items.Count > 0) cboStation.SelectedIndex = 0;

        btnStart.Click += BtnStart_Click;
        btnStop.Click += (_, _) => Stop();
        FormClosing += (_, _) => _cts?.Cancel();      // chỉ hủy, KHÔNG chờ task (tránh deadlock với Invoke)
    }

    private void BtnStart_Click(object sender, EventArgs e)
    {
        if (cboStation.SelectedItem == null) return;

        // Đọc control TRÊN UI THREAD trước khi giao cho thread phụ
        string station = cboStation.SelectedItem.ToString();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        btnStart.Enabled = false;
        btnStop.Enabled = true;
        cboStation.Enabled = false;

        // Tiến trình chạy ngầm: lặp định kỳ 2 giây/lần
        Task.Run(async () =>
        {
            int cycles = 0;
            while (!token.IsCancellationRequested)
            {
                TelemetryDTO data = _bus.Collect(station);   // sinh ngẫu nhiên điện áp & mức tải
                cycles++;
                OnNewTelemetry(data, cycles);                // chuyển về UI Thread an toàn

                try { await Task.Delay(ScadaBUS.IntervalMs, token); }
                catch (OperationCanceledException) { break; }
            }
        }, token);
    }

    /// <summary>
    /// Được gọi từ Thread phụ. KHÔNG truy cập control trực tiếp:
    /// kiểm tra InvokeRequired rồi Invoke về Main UI Thread.
    /// </summary>
    private void OnNewTelemetry(TelemetryDTO data, int cycles)
    {
        if (IsDisposed || Disposing) return;

        if (this.InvokeRequired)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    UpdateDashboard(data, cycles);
                }));
            }
            catch (ObjectDisposedException) { }          // form đã đóng giữa chừng
            catch (InvalidOperationException) { }        // handle chưa/không còn tồn tại
            return;
        }
        UpdateDashboard(data, cycles);
    }

    // Luôn chạy trên UI Thread
    private void UpdateDashboard(TelemetryDTO d, int cycles)
    {
        lblVoltage.Text = $"Điện áp: {d.VoltageKv:0.00} kV";
        lblLoadPercent.Text = $"Mức mang tải: {d.LoadPercent:0.0} %";

        if (d.IsOverload)
        {
            lblLoadPercent.ForeColor = Color.Red;
            lblAlert.Text = "CẢNH BÁO: VƯỢT NGƯỠNG AN TOÀN (> 85%)!";
            dgvGridLogs.Rows.Insert(0, d.Time.ToString("HH:mm:ss"), d.StationName,
                d.VoltageKv.ToString("0.00"), d.LoadPercent.ToString("0.0"), "Quá tải");
            dgvGridLogs.Rows[0].DefaultCellStyle.ForeColor = Color.Red;
        }
        else
        {
            lblLoadPercent.ForeColor = Color.Green;
            lblAlert.Text = "";
        }

        lblLastTime.Text = "Gói tin cuối: " + d.Time.ToString("HH:mm:ss");
        lblCycles.Text = "Tổng số chu kỳ đã quét: " + cycles;
    }

    private void Stop()
    {
        _cts?.Cancel();
        btnStart.Enabled = true;
        btnStop.Enabled = false;
        cboStation.Enabled = true;
    }
}
