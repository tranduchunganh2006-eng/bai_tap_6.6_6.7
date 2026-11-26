using AtmBanking.BUS;

namespace AtmBanking.GUI;

/// <summary>Form cha MDI - IsMdiContainer = true.</summary>
public class frmBankAtmMain : Form
{
    private const string UnlockPassword = "123456";

    private readonly MenuStrip menu = new();
    private readonly StatusStrip status = new();
    private readonly ToolStripStatusLabel lblTeller = new() { Text = "Giao dịch viên trực: (chưa đăng nhập)" };
    private readonly ToolStripStatusLabel lblCore = new() { Text = "Core Banking: đang kiểm tra...", Spring = true, TextAlign = ContentAlignment.MiddleRight };
    private readonly ToolStripProgressBar pgb = new() { Width = 160, Visible = false };
    private readonly AtmStationBUS _bus = new();

    public frmBankAtmMain()
    {
        Text = "Hệ thống điều hành mạng lưới cây ATM - Ngân hàng";
        IsMdiContainer = true;                       // YÊU CẦU: thuộc tính IsMdiContainer = true
        WindowState = FormWindowState.Maximized;
        BuildMenu();
        BuildStatusBar();
        Shown += async (_, _) => { DoLogin(); await RefreshCoreStatusAsync(); };
    }

    // ---------- MenuStrip ----------
    private void BuildMenu()
    {
        var mnuHeThong = new ToolStripMenuItem("Hệ thống");
        mnuHeThong.DropDownItems.Add("Đăng xuất", null, (_, _) => Logout());
        mnuHeThong.DropDownItems.Add("Khóa màn hình", null, (_, _) => LockScreen());
        mnuHeThong.DropDownItems.Add(new ToolStripSeparator());
        mnuHeThong.DropDownItems.Add("Thoát", null, (_, _) => Close());

        var mnuAtm = new ToolStripMenuItem("Quản lý ATM");
        mnuAtm.DropDownItems.Add("Danh sách Trạm ATM && Tồn quỹ", null, (_, _) => OpenOrActivate<frmAtmStations>());
        mnuAtm.DropDownItems.Add("Đối soát Giao dịch Cuối ngày", null, (_, _) => OpenOrActivate<frmAtmReconciliation>());

        var mnuCuaSo = new ToolStripMenuItem("Cửa sổ");
        mnuCuaSo.DropDownItems.Add("Xếp tầng (Cascade)", null, (_, _) => LayoutMdi(MdiLayout.Cascade));
        mnuCuaSo.DropDownItems.Add("Xếp ngang (Tile Horizontal)", null, (_, _) => LayoutMdi(MdiLayout.TileHorizontal));
        mnuCuaSo.DropDownItems.Add("Xếp dọc (Tile Vertical)", null, (_, _) => LayoutMdi(MdiLayout.TileVertical));
        mnuCuaSo.DropDownItems.Add(new ToolStripSeparator());
        mnuCuaSo.DropDownItems.Add("Đóng tất cả", null, (_, _) => CloseAllChildren());

        menu.Items.AddRange(new ToolStripItem[] { mnuHeThong, mnuAtm, mnuCuaSo });
        menu.MdiWindowListItem = mnuCuaSo;            // liệt kê các Form con đang mở trong menu Cửa sổ
        MainMenuStrip = menu;
        Controls.Add(menu);
    }

    private void BuildStatusBar()
    {
        status.Items.AddRange(new ToolStripItem[] { lblTeller, new ToolStripStatusLabel("|"), lblCore, pgb });
        Controls.Add(status);
    }

    // ---------- Điều hướng MDI chuẩn mực: chống mở trùng ----------
    public T OpenOrActivate<T>() where T : Form, new()
    {
        foreach (Form child in this.MdiChildren)       // duyệt qua this.MdiChildren
        {
            if (child is T existing)
            {
                if (existing.WindowState == FormWindowState.Minimized)
                    existing.WindowState = FormWindowState.Normal;
                existing.Activate();                   // đã tồn tại -> đưa lên trên cùng
                return existing;
            }
        }
        var frm = new T { MdiParent = this };          // chưa có -> khởi tạo mới
        frm.Show();
        return frm;
    }

    private void CloseAllChildren()
    {
        foreach (var c in MdiChildren.ToArray()) c.Close();
    }

    // ---------- Hệ thống ----------
    private void DoLogin()
    {
        var name = PromptForm.Ask("Đăng nhập", "Họ tên giao dịch viên trực:", false, allowCancel: false);
        lblTeller.Text = "Giao dịch viên trực: " + (string.IsNullOrWhiteSpace(name) ? "Nguyễn Văn A" : name);
    }

    private void Logout()
    {
        if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        CloseAllChildren();
        lblTeller.Text = "Giao dịch viên trực: (chưa đăng nhập)";
        DoLogin();
    }

    private void LockScreen()
    {
        while (true)
        {
            var pw = PromptForm.Ask("Màn hình đã khóa", "Nhập mật khẩu để mở khóa (mặc định 123456):", true, allowCancel: false);
            if (pw == UnlockPassword) break;
            MessageBox.Show("Sai mật khẩu!", "Khóa màn hình", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task RefreshCoreStatusAsync()
    {
        bool ok = await _bus.CheckCoreBankingAsync();
        lblCore.Text = ok ? "Core Banking: Đã kết nối" : "Core Banking: MẤT KẾT NỐI";
        lblCore.ForeColor = ok ? Color.DarkGreen : Color.Red;
    }

    // Form con gọi để cập nhật ToolStripProgressBar trên Form cha
    public void SetProgress(int value)
    {
        if (IsDisposed) return;
        pgb.Visible = value > 0 && value < 100;
        pgb.Value = Math.Clamp(value, 0, 100);
    }
}
