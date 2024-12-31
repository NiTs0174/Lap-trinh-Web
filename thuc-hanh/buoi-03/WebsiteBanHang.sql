Create database WebBanHang
go
use WebBanHang
go
CREATE TABLE Products(
	[Id] [int] NOT NULL primary key,
	[Name] [nvarchar](50) NULL,
	[Price] [decimal](18, 0) NULL,
	[Description] [nvarchar](50) NULL,
	[ImageUrl] [nvarchar](50) NULL,
	[CategoryId] [int] NULL
) ON [PRIMARY]
CREATE TABLE Categories(
	[Id] [int] NOT NULL primary key,
	[Name] [nvarchar](50) NULL
) ON [PRIMARY]
CREATE TABLE ProductImages(
	[Id] [int] NOT NULL primary key,
	[Url] [nvarchar](50) NULL,
	[ProductID] [int] NULL
) ON [PRIMARY]