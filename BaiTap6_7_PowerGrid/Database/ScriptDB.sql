-- Bai tap 6.7 - Trung tam dieu do & giam sat luoi dien thoi gian thuc
-- Ung dung van chay duoc neu KHONG co CSDL (DAL tu dung du lieu mac dinh va bo qua ghi log DB).
IF DB_ID(N'PowerGridDb') IS NULL CREATE DATABASE PowerGridDb;
GO
USE PowerGridDb;
GO
IF OBJECT_ID(N'dbo.OverloadLogs', N'U') IS NOT NULL DROP TABLE dbo.OverloadLogs;
IF OBJECT_ID(N'dbo.Substations', N'U') IS NOT NULL DROP TABLE dbo.Substations;
GO
CREATE TABLE dbo.Substations (
    Id   INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);
CREATE TABLE dbo.OverloadLogs (
    Id          INT IDENTITY PRIMARY KEY,
    StationName NVARCHAR(100) NOT NULL,
    LoggedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    VoltageKv   FLOAT         NOT NULL,
    LoadPercent FLOAT         NOT NULL
);
GO
INSERT INTO dbo.Substations(Name) VALUES
(N'Trạm 110kV Hà Đông'), (N'Trạm 220kV Chèm'), (N'Trạm 110kV Thành Công');
GO
