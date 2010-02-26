CREATE PROCEDURE  AppendNewFile
	@UserID bigint,
	@FileID bigint,
	@FileSize bigint,
	@TempHash nvarchar(512),
	@HashCode nvarchar(32),
	@HashID bigint output,
	@Delete bit output,
	@StoreDir nvarchar(256) output
AS

IF @HashCode IS NULL OR @HashCode = ''
	RETURN
	
DECLARE @ParentID bigint
DECLARE @FileName nvarchar(512)
DECLARE @OldFileSize bigint
DECLARE @OldHashID bigint

SELECT @ParentID=ParentID, @FileName=FileName,
	@OldFileSize=FileSize, @OldHashID=HashID
FROM Files WITH(NOLOCK)
WHERE UserID=@UserID AND FileID=@FileID

IF @@ROWCOUNT<=0
	RETURN
	
IF @FileSize<=@OldFileSize
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

UPDATE Files WITH(ROWLOCK)
SET FileSize=@FileSize, HashID=@HashID,
	UpdateTime=GetDate(), TempHash=@TempHash
WHERE UserID=@UserID AND FileID=@FileID

IF @@ERROR>0 OR @@ROWCOUNT<=0
	RETURN
	
BEGIN TRY	
	IF NOT EXISTS (SELECT FileID
		FROM Files WITH(NOLOCK)
		WHERE HashID=@OldHashID)
	BEGIN

		DELETE FROM HashMap WITH(ROWLOCK)
		WHERE HashID=@OldHashID
		
		 -- Delete hashID successfully, so delete physical file	
		IF @@ERROR = 0
			SET @Delete = 1
	END
END TRY
BEGIN CATCH
	SET @Delete = 0
END CATCH

SET @FileSize = @FileSize - @OldFileSize
	
EXEC UpdateFolderStatusByFile @UserID, @ParentID, 0, @FileSize

GO