-- Bai tap 6.6 - He thong MDI dieu hanh mang luoi cay ATM
-- Chay bang SSMS hoac: sqlcmd -S "(localdb)\MSSQLLocalDB" -i ScriptDB.sql
IF DB_ID(N'AtmDb') IS NULL CREATE DATABASE AtmDb;
GO
USE AtmDb;
GO
IF OBJECT_ID(N'dbo.AtmStations', N'U') IS NOT NULL DROP TABLE dbo.AtmStations;
GO
CREATE TABLE dbo.AtmStations (
    MaTram    NVARCHAR(20)  NOT NULL PRIMARY KEY,
    DiaChi    NVARCHAR(200) NOT NULL,
    TonQuy    DECIMAL(18,0) NOT NULL DEFAULT 0,
    HoatDong  BIT           NOT NULL DEFAULT 1,
    AnhPath   NVARCHAR(500) NULL
);
GO
INSERT INTO dbo.AtmStations (MaTram, DiaChi, TonQuy, HoatDong) VALUES
(N'ATM001', N'12 Duy Tan, Cau Giay, Ha Noi',        500000000, 1),
(N'ATM002', N'99 Tran Duy Hung, Cau Giay, Ha Noi',  320000000, 1),
(N'ATM003', N'25 Nguyen Trai, Thanh Xuan, Ha Noi',   45000000, 0),
(N'ATM004', N'1 Pham Van Dong, Bac Tu Liem, Ha Noi',780000000, 1);
GO
