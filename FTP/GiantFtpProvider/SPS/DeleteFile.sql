ALTER PROCEDURE DeleteFile
	@UserID BIGINT,
	@FileID BIGINT
AS
	
DECLARE @HashID BIGINT
DECLARE @ParentID BIGINT
DECLARE @FileSize BIGINT

SELECT @HashID=HashID, @ParentID=ParentID, @FileSize=FileSize
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
	
DELETE FROM Files WITH(ROWLOCK)
WHERE FileID=@FileID

IF @@ERROR<>0
	RETURN

DECLARE @DELETE INT

SET @DELETE = 0

BEGIN TRY	
	IF NOT EXISTS (SELECT FileID
		FROM Files WITH(NOLOCK)
		WHERE HashID=@HashID)
	BEGIN

		DELETE FROM HashMap WITH(ROWLOCK)
		WHERE HashID=@HashID
		
		 -- Delete hashID successfully, so delete physical file	
		IF @@ERROR = 0
			SET @DELETE = 1
	END
END TRY
BEGIN CATCH
	SET @DELETE = 0
END CATCH

EXEC UpdateFolderStatusByFile @UserID, @ParentID, -1, @FileSize

SELECT StoragePath, @HashID 'HashID', @DELETE 'DeletePhysicalFile'
FROM StorageSets WITH(NOLOCK)
WHERE StorageID=@StorageID

GO