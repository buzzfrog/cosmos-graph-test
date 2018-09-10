CREATE OR ALTER PROCEDURE dbo. usp_InsertGraphElements
(
	@tvpAssetType dbo.AssetType READONLY,
	@tvpChildType dbo.ChildType READONLY
)
AS
    SET NOCOUNT ON  

    INSERT INTO dbo.Asset (ID, level, createdAt, name, parentID, propertiesJSON)
    SELECT ID, level, createdAt, name, parentID, propertiesJSON FROM @tvpAssetType;



GO