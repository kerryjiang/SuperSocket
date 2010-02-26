CREATE PROCEDURE UpdateFolderStatusByFile
	@UserID bigint,
	@FolderID bigint,
	@Operation smallint,
	@FileSize bigint
AS

IF @Operation<-1 OR @Operation>1
	RETURN
	
DECLARE @ParentID BIGINT

SET @ParentID=@FolderID

WHILE @ParentID>0
BEGIN
	
	IF @Operation=1--upload new
	BEGIN
		UPDATE Folders WITH(ROWLOCK)
		SET SubFileCount = SubFileCount+1,
			FolderSize = FolderSize+@FileSize,
			UpdateTime = GetDate()
		WHERE UserID=@UserID AND FolderID=@ParentID	
	END
	ELSE IF @Operation=0 --append
	BEGIN
		UPDATE Folders WITH(ROWLOCK)
		SET FolderSize = FolderSize+@FileSize,
			UpdateTime = GetDate()
		WHERE UserID=@UserID AND FolderID=@ParentID	
	END
	ELSE --delete
	BEGIN
		UPDATE Folders WITH(ROWLOCK)
		SET SubFileCount = SubFileCount-1,
			FolderSize = FolderSize-@FileSize,
			UpdateTime = GetDate()
		WHERE UserID=@UserID AND FolderID=@ParentID
	END
	
	
	SET @ParentID = 0
	
	SELECT @ParentID=ParentID
	FROM Folders WITH(NOLOCK)
	WHERE UserID=@UserID AND FolderID=@ParentID
	
END

GO