CREATE PROCEDURE  UploadNewFile
	@UserID bigint,
	@ParentID bigint,
	@FileName nvarchar(512),
	@FileSize bigint,
	@TempHash nvarchar(512),
	@HashCode nvarchar(32),
	@FileID bigint output,
	@HashID bigint output,
	@StoreDir nvarchar(256) output
AS

IF @HashCode IS NULL OR @HashCode = ''
	RETURN
	
DECLARE @StorageID INT
	
SELECT @HashID=HashID, @StorageID=StorageID
FROM HashMap WITH(NOLOCK)
WHERE HashCode=@HashCode

IF @HashID IS NULL OR @HashID<=0
BEGIN

	SELECT TOP 1 @StorageID=StorageID, @StoreDir=StoragePath
	FROM StorageSets WITH(NOLOCK)
	WHERE Enabled=1 ORDER BY StorageID DESC
	
	IF @StorageID IS NULL OR @StorageID<=0
		RETURN
		
	INSERT INTO HashMap(HashCode, StorageID)
	VALUES (@HashCode, @StorageID)
	
	IF @@ERROR>0
		RETURN
	
	SET @HashID =  @@Identity	
END
ELSE
BEGIN
	SELECT @StoreDir=StoragePath
	FROM StorageSets WITH(NOLOCK)
	WHERE StorageID=@StorageID
END

INSERT INTO Files(UserID, ParentID, FileName, FileSize,
	CreateTime, UpdateTime, TempHash, HashID)
VALUES (@UserID, @ParentID, @FileName, @FileSize
	, GetDate(), GetDate(), @TempHash, @HashID)

IF @@ERROR>0
	RETURN
	
SET @FileID=@@Identity
	
EXEC UpdateFolderStatusByFile @UserID, @ParentID, 1, @FileSize

GO