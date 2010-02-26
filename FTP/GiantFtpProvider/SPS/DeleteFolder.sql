CREATE PROCEDURE DeleteFolder
	@UserID bigint,
	@FolderID bigint
AS

SELECT @FolderID=FolderID
FROM Folders WITH(NOLOCK)
WHERE UserID=@UserID AND FolderID=@FolderID

IF EXISTS (SELECT FileID FROM Files WITH(NOLOCK)
	WHERE UserID=@UserID AND ParentID=@FolderID)
BEGIN
	SELECT '1'
END

IF EXISTS (SELECT FolderID FROM Folders WITH(NOLOCK)
	WHERE UserID=@UserID AND ParentID=@FolderID)
BEGIN
	SELECT '2'
END

DELETE FROM Folders WITH(ROWLOCK)
WHERE UserID=@UserID AND FolderID=@FolderID

SELECT '0'

GO
	