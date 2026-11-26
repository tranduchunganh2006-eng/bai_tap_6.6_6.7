namespace AtmBanking.GUI;

/// <summary>Hop thoai nhap lieu nho (dang nhap / mo khoa man hinh).</summary>
public class PromptForm : Form
{
    private readonly TextBox _txt = new() { Left = 20, Top = 48, Width = 300 };

    public string Value => _txt.Text.Trim();

    public PromptForm(string title, string label, bool isPassword, bool allowCancel = true)
    {
        Text = title;
        ClientSize = new Size(340, 130);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = MinimizeBox = false;
        ControlBox = allowCancel;

        var lbl = new Label { Text = label, Left = 20, Top = 20, AutoSize = true };
        _txt.UseSystemPasswordChar = isPassword;
        var ok = new Button { Text = "OK", Left = 160, Top = 85, Width = 75, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Hủy", Left = 245, Top = 85, Width = 75, DialogResult = DialogResult.Cancel, Enabled = allowCancel };
        Controls.AddRange(new Control[] { lbl, _txt, ok, cancel });
        AcceptButton = ok;
        if (allowCancel) CancelButton = cancel;
    }

    public static string Ask(string title, string label, bool isPassword, bool allowCancel = true)
    {
        using var f = new PromptForm(title, label, isPassword, allowCancel);
        return f.ShowDialog() == DialogResult.OK ? f.Value : null;
    }
}
