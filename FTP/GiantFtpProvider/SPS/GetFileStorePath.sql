CREATE PROCEDURE GetFileStorePath
	@UserID bigint,
	@FileID bigint
AS

DECLARE @HashID BIGINT

SELECT @HashID=HashID
FROM Files WITH(NOLOCK)
WHERE UserID=@UserID AND FileID=@FileID

IF @HashID IS NULL OR @HashID<=0
	RETURN

DECLARE @StorageID INT

SELECT @StorageID=StorageID
FROM HashMap WITH(NOLOCK)
WHERE HashID=@HashID

IF @StorageID IS NULL OR @StorageID<=0
	RETURN
	
SELECT StoragePath, @HashID, @StorageID
FROM StorageSets WITH(NOLOCK)
WHERE StorageID=@StorageID

GO