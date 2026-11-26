using AtmBanking.BUS;
using AtmBanking.DTO;

namespace AtmBanking.GUI;

/// <summary>Form con 1: quản lý trạm ATM, tồn quỹ, ảnh hiện trường (OpenFileDialog).</summary>
public class frmAtmStations : Form
{
    private readonly AtmStationBUS _bus = new();
    private readonly TextBox txtMa = new() { Left = 110, Top = 15, Width = 160 };
    private readonly TextBox txtDiaChi = new() { Left = 110, Top = 45, Width = 300 };
    private readonly NumericUpDown numTonQuy = new() { Left = 110, Top = 75, Width = 160, Maximum = 1_000_000_000_000M, ThousandsSeparator = true };
    private readonly CheckBox chkHoatDong = new() { Left = 110, Top = 105, Text = "Đang hoạt động", Checked = true, AutoSize = true };
    private readonly PictureBox pic = new() { Left = 440, Top = 15, Width = 190, Height = 130, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
    private readonly Button btnAnh = new() { Left = 440, Top = 150, Width = 190, Text = "Đính kèm ảnh hiện trường..." };
    private readonly Button btnThem = new() { Left = 20, Top = 140, Width = 80, Text = "Thêm" };
    private readonly Button btnSua = new() { Left = 105, Top = 140, Width = 80, Text = "Sửa" };
    private readonly Button btnXoa = new() { Left = 190, Top = 140, Width = 80, Text = "Xóa" };
    private readonly Button btnMoi = new() { Left = 275, Top = 140, Width = 80, Text = "Làm mới" };
    private readonly DataGridView dgv = new() { Dock = DockStyle.Fill };
    private string _anhPath;
    private bool _clearing;

    public frmAtmStations()
    {
        Text = "Danh sách Trạm ATM & Tồn quỹ";
        Size = new Size(720, 520);

        var top = new Panel { Dock = DockStyle.Top, Height = 190 };
        top.Controls.AddRange(new Control[]
        {
            new Label { Text = "Mã trạm:", Left = 20, Top = 18, AutoSize = true }, txtMa,
            new Label { Text = "Địa chỉ đặt máy:", Left = 20, Top = 48, AutoSize = true }, txtDiaChi,
            new Label { Text = "Tồn quỹ (VND):", Left = 20, Top = 78, AutoSize = true }, numTonQuy,
            new Label { Text = "Trạng thái:", Left = 20, Top = 108, AutoSize = true }, chkHoatDong,
            pic, btnAnh, btnThem, btnSua, btnXoa, btnMoi
        });

        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(dgv);
        Controls.Add(top);

        btnAnh.Click += (_, _) => ChooseImage();
        btnThem.Click += (_, _) => Save(true);
        btnSua.Click += (_, _) => Save(false);
        btnXoa.Click += (_, _) => Remove();
        btnMoi.Click += (_, _) => ClearInput();
        dgv.SelectionChanged += (_, _) => ShowSelected();
        Load += (_, _) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            dgv.DataSource = _bus.GetAll();
            Rename("MaTram", "Mã trạm"); Rename("DiaChi", "Địa chỉ đặt máy");
            Rename("TonQuy", "Số tiền tồn quỹ"); Rename("HoatDong", "Hoạt động"); Rename("AnhPath", "Ảnh");
            dgv.Columns["TonQuy"].DefaultCellStyle.Format = "N0";
            dgv.Columns["TonQuy"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không đọc được CSDL. Hãy chạy Database/ScriptDB.sql trước.\n\n" + ex.Message,
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Rename(string col, string header) => dgv.Columns[col].HeaderText = header;

    private void ShowSelected()
    {
        if (_clearing) return;
        if (dgv.CurrentRow?.DataBoundItem is not AtmStationDTO s) return;
        txtMa.Text = s.MaTram; txtMa.ReadOnly = true;
        txtDiaChi.Text = s.DiaChi;
        numTonQuy.Value = s.TonQuy;
        chkHoatDong.Checked = s.HoatDong;
        _anhPath = s.AnhPath;
        ShowImage(_anhPath);
    }

    private void ChooseImage()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn ảnh chụp hiện trường trạm ATM",
            Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
        };
        if (ofd.ShowDialog() != DialogResult.OK) return;
        _anhPath = ofd.FileName;
        ShowImage(_anhPath);
    }

    private void ShowImage(string path)
    {
        pic.Image?.Dispose();
        pic.Image = null;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        using var tmp = Image.FromFile(path);
        pic.Image = new Bitmap(tmp);                  // không giữ khóa file
    }

    private AtmStationDTO ReadInput() => new()
    {
        MaTram = txtMa.Text.Trim(),
        DiaChi = txtDiaChi.Text.Trim(),
        TonQuy = numTonQuy.Value,
        HoatDong = chkHoatDong.Checked,
        AnhPath = _anhPath
    };

    private void Save(bool isNew)
    {
        try
        {
            var s = ReadInput();
            var err = isNew ? _bus.Add(s) : _bus.Update(s);
            if (err != null) { MessageBox.Show(err, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            LoadData();
            ClearInput();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void Remove()
    {
        if (string.IsNullOrEmpty(txtMa.Text)) return;
        if (MessageBox.Show($"Xóa trạm {txtMa.Text}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        try { _bus.Delete(txtMa.Text); LoadData(); ClearInput(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ClearInput()
    {
        _clearing = true;
        txtMa.Clear(); txtMa.ReadOnly = false; txtDiaChi.Clear();
        numTonQuy.Value = 0; chkHoatDong.Checked = true; _anhPath = null;
        pic.Image?.Dispose(); pic.Image = null;
        dgv.ClearSelection();
        _clearing = false;
    }
}
