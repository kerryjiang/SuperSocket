CREATE PROCEDURE RenameFolder
	@UserID BIGINT,
	@FolderID BIGINT,
	@NewFolderName NVARCHAR(512)
AS

DECLARE @ParentID BIGINT
DECLARE @OldName NVARCHAR

SELECT @ParentID=ParentID
FROM Folders WITH(NOLOCK)
WHERE UserID=@UserID AND FolderID=@FolderID

IF EXISTS(SELECT FileID FROM Files WITH(NOLOCK)
	WHERE UserID=@UserID AND FileName=@NewFolderName)
BEGIN
	SELECT '1'
	RETURN
END

IF EXISTS(SELECT FolderID FROM Folders WITH(NOLOCK)
	WHERE UserID=@UserID AND FolderName=@NewFolderName)
BEGIN
	SELECT '2'
	RETURN
END

UPDATE Folders WITH(ROWLOCK)
SET FolderName=@NewFolderName, UpdateTime=GetDate()
WHERE UserID=@UserID AND FolderID=@FolderID

SELECT '0'


GO