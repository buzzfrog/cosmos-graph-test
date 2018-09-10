IF OBJECT_ID('dbo.Asset', 'U') IS NOT NULL 
  DROP TABLE dbo.Asset;
GO
CREATE TABLE Asset 
(
  ID VARCHAR(100) PRIMARY KEY, 
  level INT,
  createdAt DATETIME2,
  name VARCHAR(100),
  parentID VARCHAR(100),
  propertiesJSON NVARCHAR(MAX)
) AS NODE;
GO

IF TYPE_ID('dbo.AssetType') IS NOT NULL
	DROP TYPE dbo.AssetType;
GO
CREATE TYPE AssetType AS TABLE
(
  ID VARCHAR(100) PRIMARY KEY, 
  level INT,
  createdAt DATETIME2,
  name VARCHAR(100),
  parentID VARCHAR(100),
  propertiesJSON NVARCHAR(MAX)
)
GO

IF OBJECT_ID('dbo.Child', 'U') IS NOT NULL 
  DROP TABLE dbo.Child;
GO
CREATE TABLE Child AS EDGE;
GO

IF TYPE_ID('dbo.ChildType') IS NOT NULL
	DROP TYPE dbo.ChildType;
GO
CREATE TYPE ChildType AS TABLE
(
  fromID VARCHAR(100),
  toID VARCHAR(100)
)
GO