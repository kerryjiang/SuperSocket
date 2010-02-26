CREATE PROCEDURE GetHashStorePath
	@HashID bigint
AS

IF @HashID IS NULL OR @HashID<=0
	RETURN

DECLARE @StorageID INT
DECLARE @HashCode nvarchar(32)

SELECT @StorageID=StorageID, @HashCode=HashCode
FROM HashMap WITH(NOLOCK)
WHERE HashID=@HashID

IF @StorageID IS NULL OR @StorageID<=0
	RETURN
	
SELECT StoragePath, @HashCode HashCode
FROM StorageSets WITH(NOLOCK)
WHERE StorageID=@StorageID

GO