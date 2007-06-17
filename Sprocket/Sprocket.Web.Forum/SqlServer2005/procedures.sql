IF OBJECT_ID(N'dbo.Forum_SelectByCode') IS NOT NULL
	DROP PROCEDURE Forum_SelectByCode
go
CREATE PROCEDURE dbo.Forum_SelectByCode
	@ForumCode nvarchar(50)
AS
BEGIN
	SELECT *
	  FROM Forum
	 WHERE ForumCode = @ForumCode
END
go

IF OBJECT_ID(N'dbo.ForumCategory_SelectByCode') IS NOT NULL
	DROP PROCEDURE ForumCategory_SelectByCode
go
CREATE PROCEDURE dbo.ForumCategory_SelectByCode
	@CategoryCode nvarchar(50)
AS
BEGIN
	SELECT *
	  FROM ForumCategory
	 WHERE CategoryCode = @CategoryCode
END
go

IF OBJECT_ID(N'dbo.Forum_SelectByURLToken') IS NOT NULL
	DROP PROCEDURE Forum_SelectByURLToken
go
CREATE PROCEDURE dbo.Forum_SelectByURLToken
	@URLToken nvarchar(50)
AS
BEGIN
	SELECT *
	  FROM Forum
	 WHERE URLToken = @URLToken
END
go

IF OBJECT_ID(N'dbo.ForumCategory_SelectByURLToken') IS NOT NULL
	DROP PROCEDURE ForumCategory_SelectByURLToken
go
CREATE PROCEDURE dbo.ForumCategory_SelectByURLToken
	@URLToken nvarchar(50)
AS
BEGIN
	SELECT *
	  FROM ForumCategory
	 WHERE URLToken = @URLToken
END
go
IF OBJECT_ID(N'dbo.ForumCategory_ListForums') IS NOT NULL
	DROP PROCEDURE ForumCategory_ListForums
go
CREATE PROCEDURE dbo.ForumCategory_ListForums
	@ForumCategoryID bigint
AS
BEGIN
	SELECT *
	  FROM Forum
	 WHERE ForumCategoryID = @ForumCategoryID
  ORDER BY Rank
END
go

IF OBJECT_ID(N'dbo.ForumCategory_ListForumsByCategoryCode') IS NOT NULL
	DROP PROCEDURE ForumCategory_ListForumsByCategoryCode
go
CREATE PROCEDURE dbo.ForumCategory_ListForumsByCategoryCode
	@CategoryCode nvarchar(50)
AS
BEGIN
	DECLARE @ID bigint
	SELECT @ID = ForumCategoryID
	  FROM ForumCategory
	 WHERE CategoryCode = @CategoryCode
	 
	SELECT *
	  FROM Forum
	 WHERE ForumCategoryID = @ID
  ORDER BY Rank
END
go

IF OBJECT_ID(N'dbo.ForumCategory_List') IS NOT NULL
	DROP PROCEDURE ForumCategory_List
go
CREATE PROCEDURE dbo.ForumCategory_List
	@InternalUseOnly bit = NULL
AS
BEGIN
	SELECT *
	  FROM ForumCategory
	 WHERE @InternalUseOnly IS NULL OR InternalUseOnly = @InternalUseOnly
  ORDER BY Rank
END
go

IF OBJECT_ID(N'dbo.ForumCategory_ListForumSummary') IS NOT NULL
	DROP PROCEDURE ForumCategory_ListForumSummary
go
CREATE PROCEDURE dbo.ForumCategory_ListForumSummary
	@CategoryCode nvarchar(50)
AS
BEGIN
	DECLARE @ID bigint
	SELECT @ID = ForumCategoryID
	  FROM ForumCategory
	 WHERE CategoryCode = @CategoryCode
	 
	SELECT *,
			(SELECT COUNT(*)
			   FROM ForumTopic t
			  WHERE t.ForumID = f.ForumID
			    AND t.ModerationState = 1) AS [TopicCount],
			    
			(SELECT COUNT(*)
			   FROM ForumTopicMessage m
		 INNER JOIN ForumTopic t
				 ON t.ForumTopicID = m.ForumTopicID
			  WHERE t.ForumID = f.ForumID
			    AND m.ModerationState IN (1,2)
			    AND t.ModerationState IN (1,2)) AS [ReplyCount],
			
			(SELECT u.Username
			   FROM Users u
			  WHERE u.UserID = (SELECT TOP 1 AuthorUserID
								  FROM ForumTopic t
								 WHERE t.ModerationState IN (1,2)
							  ORDER BY t.DateCreated DESC)) AS [AuthorUsername],
			
			(SELECT TOP 1 m.DateCreated
			   FROM ForumTopicMessage m
		 INNER JOIN ForumTopic t
				 ON t.ForumTopicID = m.ForumTopicID
			  WHERE t.ForumID = f.ForumID
			    AND m.ModerationState IN (1,2)
			    AND t.ModerationState IN (1,2)
		   ORDER BY m.DateCreated DESC) AS [LastReplyTime]
			
	  FROM Forum f
	 WHERE f.ForumCategoryID = @ID
  ORDER BY f.Rank
END
go

IF OBJECT_ID(N'dbo.ForumTopic_ListForumTopicMessages') IS NOT NULL
	DROP PROCEDURE ForumTopic_ListForumTopicMessages
go
CREATE PROCEDURE dbo.ForumTopic_ListForumTopicMessages
	@ForumTopicID bigint,
	@RecordsPerPage int,
	@PageNumber int,
	@ReverseOrder bit,
	@HideUnmoderatedMessages bit,
	@Total int=NULL OUTPUT
AS
BEGIN
	IF @HideUnmoderatedMessages IS NULL
		SET @HideUnmoderatedMessages = 0
	IF @HideUnmoderatedMessages IS NULL
		SET @HideUnmoderatedMessages = 0
	
	SELECT @Total = COUNT(*)
	  FROM ForumTopicMessage
	 WHERE ForumTopicID = @ForumTopicID AND (@HideUnmoderatedMessages = 0 OR ModerationState IN (1,2))
	 
	DECLARE @n1 int, @n2 int
	IF @RecordsPerPage > 0
	BEGIN
		SET @n1 = (@PageNumber-1) * @RecordsPerPage + 1
		SET @n2 = @n1 + @RecordsPerPage - 1;
	END
	ELSE
	BEGIN
		SET @n1 = 1
		SET @n2 = @Total
	END;

	WITH msgs AS
	(
		SELECT *,
				ROW_NUMBER() OVER (ORDER BY msg.DateCreated ASC) AS [RowIndex],
				ROW_NUMBER() OVER (ORDER BY msg.DateCreated DESC) AS [ReverseRowIndex]
		  FROM ForumTopicMessage msg
		 WHERE (@HideUnmoderatedMessages = 0 OR msg.ModerationState IN (1,2))
	)
	SELECT ForumTopicMessageID, ForumTopicID, AuthorUserID,
			CASE WHEN AuthorUserID IS NOT NULL THEN (SELECT u.Username FROM Users u WHERE u.UserID = AuthorUserID) ELSE AuthorName END as [AuthorName],
		   DateCreated, BodySource, BodyOutput, ModerationState, MarkupType
	  FROM msgs
	 WHERE (@ReverseOrder = 0 AND RowIndex BETWEEN @n1 AND @n2)
		OR (@ReverseOrder = 1 AND ReverseRowIndex BETWEEN @n1 AND @n2)
  ORDER BY CASE WHEN @ReverseOrder = 0 THEN RowIndex ELSE ReverseRowIndex END
END