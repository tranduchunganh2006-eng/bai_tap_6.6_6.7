# Bài tập 6.6 & 6.7 – WinForms .NET 8 (kiến trúc 3-Tier)

| Bài | Nội dung | Thư mục |
|---|---|---|
| 6.6 | Hệ thống MDI điều hành mạng lưới cây ATM (MDI + async/await) | `BaiTap6_6_AtmMdi/` |
| 6.7 | Trung tâm điều độ & giám sát lưới điện (Cross-thread safety) | `BaiTap6_7_PowerGrid/` |

Mỗi bài có cấu trúc: `Database/ScriptDB.sql`, `DTO`, `DAL`, `BUS`, `GUI`, và file `.sln` riêng.

## Yêu cầu môi trường
- Windows 10/11, **Visual Studio 2022** (workload *.NET desktop development*) hoặc .NET SDK 8.0
- SQL Server LocalDB (đi kèm Visual Studio). Bài 6.6 cần CSDL; bài 6.7 chạy được kể cả khi không có CSDL.

## Cách chạy
**Bài 6.6**
1. Chạy `BaiTap6_6_AtmMdi/Database/ScriptDB.sql` bằng SSMS hoặc `sqlcmd -S "(localdb)\MSSQLLocalDB" -i ScriptDB.sql`.
2. Nếu dùng SQL Server khác LocalDB, sửa `ConnectionString` trong `DAL/DbHelper.cs`.
3. Mở `AtmBanking.sln` → đặt `AtmBanking.GUI` làm Startup Project → **F5**.
4. Mật khẩu mở khóa màn hình mặc định: `123456`.

**Bài 6.7**
1. (Tuỳ chọn) chạy `BaiTap6_7_PowerGrid/Database/ScriptDB.sql` để lưu nhật ký quá tải xuống DB.
2. Mở `PowerGrid.sln` → Startup Project `PowerGrid.GUI` → **F5** (chế độ Debug).

---

## Bài 6.6 – Hệ thống MDI điều hành mạng lưới cây ATM

### Đối chiếu yêu cầu
| Yêu cầu đề bài | Cách thực hiện | File |
|---|---|---|
| Form cha `frmBankAtmMain`, `IsMdiContainer = true` | Đặt trong constructor | `GUI/frmBankAtmMain.cs` |
| MenuStrip: Hệ thống / Quản lý ATM / Cửa sổ | `BuildMenu()` | `GUI/frmBankAtmMain.cs` |
| StatusStrip: tên GDV trực, trạng thái Core Banking, `ToolStripProgressBar` | `BuildStatusBar()`, `RefreshCoreStatusAsync()`, `SetProgress()` | `GUI/frmBankAtmMain.cs` |
| Chống mở trùng Form con | `OpenOrActivate<T>()` duyệt `this.MdiChildren`, có rồi thì `Activate()`, chưa có thì tạo mới với `MdiParent = this` | `GUI/frmBankAtmMain.cs` |
| Cascade / Tile Horizontal / Tile Vertical / Đóng tất cả | `LayoutMdi(...)`, `CloseAllChildren()`, `MdiWindowListItem` | `GUI/frmBankAtmMain.cs` |
| `frmAtmStations`: Mã trạm, Địa chỉ, Tồn quỹ, Trạng thái, `OpenFileDialog` đính kèm ảnh | CRUD qua BUS → DAL (ADO.NET, tham số hóa) | `GUI/frmAtmStations.cs` |
| `frmAtmReconciliation`: nút **Chạy Đối Soát Tự Động** và **Dừng Đối Soát** | `async void` + `await` + `CancellationTokenSource` | `GUI/frmAtmReconciliation.cs` |
| Giả lập tải/đối chiếu 4–6 giây (`Task.Run` + `Task.Delay`) | `Task.Run` ở BUS, `Task.Delay` ở DAL | `BUS/ReconciliationBUS.cs`, `DAL/ReconciliationDAL.cs` |
| UI không "Not Responding", ProgressBar trên Form cha chạy theo | `IProgress<int>` cập nhật bar của Form con và `Main.SetProgress()` | `GUI/frmAtmReconciliation.cs` |

### Ảnh chụp kết quả chạy
> Chụp các màn hình sau khi chạy trên máy của bạn rồi lưu vào thư mục `Screenshots/` với đúng tên file.

**Ảnh 1 – Form cha MDI + menu + StatusStrip**
![6.6-01](Screenshots/6.6-01-main-menu.png)

**Ảnh 2 – Danh sách trạm ATM & tồn quỹ (kèm ảnh hiện trường)**
![6.6-02](Screenshots/6.6-02-stations.png)

**Ảnh 3 – Nhấn menu nhiều lần chỉ có 1 Form con (không trùng lặp)**
![6.6-03](Screenshots/6.6-03-no-duplicate.png)

**Ảnh 4 – Đang đối soát: ProgressBar trên Form cha chạy, đồng hồ UI vẫn nhảy**
![6.6-04](Screenshots/6.6-04-reconciling.png)

**Ảnh 5 – Hoàn tất / Dừng đối soát và xếp cửa sổ (Cascade/Tile)**
![6.6-05](Screenshots/6.6-05-done-tile.png)

### Cách kiểm thử
1. Mở **Quản lý ATM → Danh sách Trạm ATM & Tồn quỹ** 5 lần liên tiếp: chỉ có 1 cửa sổ, cửa sổ đó được đưa lên trên cùng.
2. Mở **Đối soát Giao dịch Cuối ngày** → nhấn **Chạy Đối Soát Tự Động**. Trong 4–6 giây: kéo cửa sổ, mở form khác, chọn Tile/Cascade. UI vẫn mượt, đồng hồ ở form đối soát vẫn nhảy, thanh tiến trình ở StatusStrip chạy.
3. Nhấn **Dừng Đối Soát** giữa chừng → log ghi "Đã DỪNG", nút trở lại trạng thái ban đầu.

---

## Bài 6.7 – Trung tâm điều độ & giám sát lưới điện (Cross-Thread Safety)

### Đối chiếu yêu cầu
| Yêu cầu đề bài | Cách thực hiện | File |
|---|---|---|
| Nút **Bắt đầu Giám sát SCADA**, **Ngắt Kết nối**, ComboBox chọn trạm | Khu vực điều khiển | `GUI/frmPowerGridMonitor.cs` |
| `lblVoltage` (kV), `lblLoadPercent` (%) | Cập nhật mỗi chu kỳ | `GUI/frmPowerGridMonitor.cs` |
| Tải > 85% → chữ **Đỏ** + cảnh báo; ổn định → **Xanh lá** | `UpdateDashboard()` | `GUI/frmPowerGridMonitor.cs` |
| StatusStrip: giờ gói tin cuối (HH:mm:ss) và tổng chu kỳ đã quét | `lblLastTime`, `lblCycles` | `GUI/frmPowerGridMonitor.cs` |
| `dgvGridLogs` cấu hình bằng code, ghi mốc thời gian quá tải | `Columns.Add(...)`, `Rows.Insert(...)` | `GUI/frmPowerGridMonitor.cs` |
| Task.Run vòng lặp ngầm 2 giây/lần; điện áp 108–115 kV, tải 50–98% | `Task.Run` + `Task.Delay(2000)` | `GUI/frmPowerGridMonitor.cs`, `DAL/TelemetryDAL.cs` |
| Thread phụ không truy cập control trực tiếp; dùng `InvokeRequired` + `Invoke(new Action(() => {...}))` | `OnNewTelemetry()` | `GUI/frmPowerGridMonitor.cs` |
| Không ném `InvalidOperationException: Cross-thread operation not valid` | Thread phụ chỉ làm việc với DTO; đọc `cboStation` trước khi giao cho thread phụ | `GUI/frmPowerGridMonitor.cs` |

### Ảnh chụp kết quả chạy (Debug – F5)
**Ảnh 1 – Giao diện ban đầu**
![6.7-01](Screenshots/6.7-01-initial.png)

**Ảnh 2 – Đang giám sát, tải ổn định (chữ xanh lá)**
![6.7-02](Screenshots/6.7-02-normal.png)

**Ảnh 3 – Tải > 85%: chữ đỏ + cảnh báo**
![6.7-03](Screenshots/6.7-03-overload.png)

**Ảnh 4 – DataGridView ghi nhận các lần quá tải, StatusStrip cập nhật**
![6.7-04](Screenshots/6.7-04-logs.png)

**Ảnh 5 – Ngắt kết nối và Visual Studio Debug không có exception**
![6.7-05](Screenshots/6.7-05-stopped-no-exception.png)

### Cách kiểm thử
1. Chạy **F5**, chọn "Trạm 220kV Chèm", nhấn **Bắt đầu Giám sát SCADA**.
2. Cứ 2 giây số liệu đổi một lần; khi tải > 85% chữ chuyển đỏ, dòng mới xuất hiện đầu bảng nhật ký.
3. Để chạy vài phút, rồi nhấn **Ngắt Kết nối** hoặc đóng form: Visual Studio không báo exception nào.

---

## Ghi chú kiến trúc 3-Tier
- **DTO**: đối tượng truyền dữ liệu thuần (không logic).
- **DAL**: truy cập dữ liệu (ADO.NET, truy vấn tham số hóa) và nguồn dữ liệu giả lập.
- **BUS**: nghiệp vụ, kiểm tra hợp lệ, điều phối `Task.Run`, xác định ngưỡng quá tải.
- **GUI**: chỉ xử lý giao diện; luôn gọi qua BUS, không gọi trực tiếp DAL.
