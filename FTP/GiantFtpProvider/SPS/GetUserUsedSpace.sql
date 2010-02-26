CREATE PROCEDURE GetUserUsedSpace
	@UserID BIGINT,
	@UserName nvarchar(50)
AS

IF @UserID<=0
BEGIN
	SELECT @UserID=UserID
	FROM FtpUsers WITH(NOLOCK)
	WHERE LowerUserName=LOWER(@UserName)
END

IF @UserID<=0
BEGIN
	SELECT 0
	RETURN
END

DECLARE @FolderSize BIGINT
DECLARE @FileSize BIGINT

SET @FolderSize = 0
SET @FileSize = 0

SELECT @FolderSize=SUM(FolderSize)
FROM Folders WITH(NOLOCK)
WHERE UserID=@UserID AND ParentID=0

SELECT @FileSize=SUM(FileSize)
FROM Files WITH(NOLOCK)
WHERE UserID=@UserID AND ParentID=0

SELECT @FolderSize+@FileSize

GO