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