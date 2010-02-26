ALTER PROCEDURE RenameFile
	@UserID BIGINT,
	@FileID BIGINT,
	@NewFileName NVARCHAR(512)
AS

DECLARE @ParentID BIGINT

SELECT @ParentID=ParentID
FROM Files WITH(NOLOCK)
WHERE UserID=@UserID AND FileID=@FileID

IF EXISTS(SELECT FileID FROM Files WITH(NOLOCK)
	WHERE UserID=@UserID AND FileName=@NewFileName)
BEGIN
	SELECT '1'
	RETURN
END

IF EXISTS(SELECT FolderID FROM Folders WITH(NOLOCK)
	WHERE UserID=@UserID AND FolderName=@NewFileName)
BEGIN
	SELECT '2'
	RETURN
END

UPDATE Files WITH(ROWLOCK)
SET FileName=@NewFileName, UpdateTime=GetDate()
WHERE UserID=@UserID AND FileID=@FileID

SELECT '0'


GO