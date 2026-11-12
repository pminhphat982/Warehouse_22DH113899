-- 1. Tạo Database (Thay MSSV bằng mã số thật của bạn)
CREATE DATABASE Warehouse_MSSV; 
GO
USE Warehouse_MSSV;
GO

-- 2. Tạo bảng Products với đúng kiểu dữ liệu yêu cầu
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL
);
GO

-- 3. Nhập 5 sản phẩm, thuộc 2 danh mục, có gắn tên sinh viên
INSERT INTO Products (ProductName, Category, Price, Quantity) VALUES 
(N'Bàn phím cơ An', N'Phụ kiện', 500000.00, 10),
(N'Chuột không dây An', N'Phụ kiện', 250000.00, 3),
(N'Lót chuột An', N'Phụ kiện', 50000.00, 0),
(N'Màn hình 24 inch An', N'Thiết bị', 3000000.00, 5),
(N'Tai nghe gaming An', N'Thiết bị', 450000.00, 2);
GO