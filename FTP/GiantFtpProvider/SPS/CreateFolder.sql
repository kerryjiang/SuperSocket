ALTER PROCEDURE CreateFolder
	@UserID BIGINT,
	@ParentID BIGINT,
	@FolderName NVARCHAR(512)
AS

IF EXISTS(SELECT FolderID FROM Folders WITH(NOLOCK)
	WHERE UserID=@UserID AND FolderName=@FolderName)
BEGIN
	SELECT 2
	RETURN
END


IF EXISTS(SELECT FileID FROM Files WITH(NOLOCK)
	WHERE UserID=@UserID AND FileName=@FolderName)
BEGIN
	SELECT 1
	RETURN
END

INSERT INTO Folders(UserID, FolderName, ParentID, SubFileCount,
	SubFolderCount, FolderSize, CreateTime, UpdateTime)
VALUES(@UserID, @FolderName, @ParentID, 0, 0, 0, GetDate(), GetDate())

SELECT 0

GO