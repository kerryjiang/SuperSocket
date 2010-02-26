CREATE PROCEDURE GetFolderIDByRelativePath
	@UserID bigint,
	@ParentID bigint,
	@RelativePath nvarchar(1000)
AS

DECLARE @Delim CHAR
SET @Delim = '/'

IF @RelativePath IS NULL OR @RelativePath = ''
BEGIN
	SELECT @ParentID
	RETURN
END

DECLARE @StartPos INT
DECLARE @Pos INT
SET @StartPos = 1
SET @Pos = 1

IF SUBSTRING(@RelativePath, 1, 1) = @Delim
BEGIN
	SET @StartPos = 2
END

DECLARE @Total INT
SET @Total = LEN(@RelativePath)

DECLARE @Node nvarchar(500)
SET @Node = ''

DECLARE @FolderID BIGINT
SET @FolderID = -1

DECLARE @NewFolderID BIGINT
SET @NewFolderID = -1

WHILE @StartPos<@Total
BEGIN
	SET @Pos = CHARINDEX(@Delim, @RelativePath, @StartPos)
	
	IF @Pos > 0
	BEGIN
	
		SET @Node = SUBSTRING(@RelativePath, @StartPos, @Pos-@StartPos)

		SELECT @NewFolderID = FolderID
		FROM Folders WITH(NOLOCK)
		WHERE UserID=@UserID AND ParentID=@ParentID AND FolderName=@Node
		
		IF @NewFolderID IS NULL OR @NewFolderID<=0
		BEGIN
			SELECT -1
			RETURN		
		END
		
		SET @StartPos = @Pos + 1
		SET @ParentID = @NewFolderID
		SET @NewFolderID = -1	
	END
	ELSE
	BEGIN
	
		SET @Node = SUBSTRING(@RelativePath, @StartPos, @Total-@StartPos+1)

		SELECT @NewFolderID = FolderID
		FROM Folders WITH(NOLOCK)
		WHERE UserID=@UserID AND ParentID=@ParentID AND FolderName=@Node
		
		
		IF @NewFolderID IS NULL OR @NewFolderID<=0
		BEGIN
			SELECT -1
			RETURN		
		END
		ELSE
		BEGIN
			SELECT @NewFolderID
			RETURN
		END		
		
	END
END

GO