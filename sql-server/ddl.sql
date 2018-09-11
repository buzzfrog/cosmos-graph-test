-- NOTE:
-- COLUMN NAMES MUST MATCH THE PROPERTIES SPECIFIED IN C# CODE
-- COLUMN NAMES ARE CASE SENSITIVE!!!

IF OBJECT_ID('dbo.Asset', 'U') IS NOT NULL 
	DROP TABLE dbo.Asset;
GO

IF OBJECT_ID('dbo.usp_InsertGraphElements', 'P') IS NOT NULL
	DROP PROCEDURE usp_InsertGraphElements
GO

IF TYPE_ID('dbo.AssetType') IS NOT NULL
	DROP TYPE dbo.AssetType;
GO

IF OBJECT_ID('dbo.Child', 'U') IS NOT NULL 
	DROP TABLE dbo.Child;
GO

IF TYPE_ID('dbo.ChildType') IS NOT NULL
	DROP TYPE dbo.ChildType;
GO

CREATE TABLE Asset 
(
	id VARCHAR(100) PRIMARY KEY, 
	partitionKey VARCHAR(100), 
	level INT,
	createdAt BIGINT,
	name VARCHAR(100),
	parentId VARCHAR(100),
	propertiesJson NVARCHAR(MAX)
) AS NODE;
GO

CREATE TYPE AssetType AS TABLE
(
	id VARCHAR(100) PRIMARY KEY, 
	partitionKey VARCHAR(100), 
	level INT,
	createdAt BIGINT,
	name VARCHAR(100),
	parentId VARCHAR(100),
	propertiesJson NVARCHAR(MAX)
)
GO

CREATE TABLE Child AS EDGE;
GO

CREATE TYPE ChildType AS TABLE
(
	fromId VARCHAR(100),
	toId VARCHAR(100)
)
GO

CREATE PROCEDURE dbo.usp_InsertGraphElements
(
	@tvpAssetType dbo.AssetType READONLY,
	@tvpChildType dbo.ChildType READONLY
)
AS
    SET NOCOUNT ON  

    INSERT INTO Asset
    SELECT ID, partitionKey, level, createdAt, name, parentID, propertiesJSON FROM @tvpAssetType;

	INSERT INTO Child 
	SELECT f.$node_id, t.$node_id
	FROM @tvpChildType tvp
	INNER JOIN Asset f ON tvp.fromID = f.ID
	INNER JOIN Asset t ON tvp.toID = t.ID

GO