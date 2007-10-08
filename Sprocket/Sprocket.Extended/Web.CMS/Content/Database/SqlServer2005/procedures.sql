---------------
-- REVISIONS --
---------------

IF OBJECT_ID(N'dbo.StoreRevisionInformation') IS NOT NULL
	DROP PROCEDURE StoreRevisionInformation
go
CREATE PROCEDURE dbo.StoreRevisionInformation
	@RevisionID bigint,
	@RevisionSourceID bigint,
	@RevisionDate datetime,
	@UserID bigint,
	@Notes nvarchar(max),
	@Hidden bit,
	@Draft bit,
	@Deleted bit
AS
-- there is never a reason to change any of the existing fields
INSERT INTO RevisionInformation
	(RevisionID, RevisionSourceID, RevisionDate, UserID, Notes, Hidden, Draft, Deleted)
VALUES
	(@RevisionID, @RevisionSourceID, @RevisionDate, @UserID, @Notes, @Hidden, @Draft, @Deleted);

go
IF OBJECT_ID(N'dbo.SelectRevisionInformation') IS NOT NULL
	DROP PROCEDURE SelectRevisionInformation
go
CREATE PROCEDURE dbo.SelectRevisionInformation
	@RevisionID bigint
AS
SELECT *
  FROM RevisionInformation
 WHERE RevisionID = @RevisionID
go

IF OBJECT_ID(N'dbo.DeleteRevisionInformation') IS NOT NULL
	DROP PROCEDURE DeleteRevisionInformation
go
CREATE PROCEDURE dbo.DeleteRevisionInformation
	@RevisionID bigint
AS
DELETE FROM RevisionInformation WHERE RevisionID = @RevisionID;

go
IF OBJECT_ID(N'dbo.ListRevisionsBySource') IS NOT NULL
	DROP PROCEDURE ListRevisionsBySource
go
CREATE PROCEDURE dbo.ListRevisionsBySource
	@RevisionSourceID bigint
AS
	SELECT *
	  FROM RevisionInformation
	 WHERE RevisionSourceID = @RevisionSourceID
  ORDER BY RevisionDate DESC
go

IF OBJECT_ID(N'dbo.DeleteDraftRevisions') IS NOT NULL
	DROP PROCEDURE DeleteDraftRevisions
go
CREATE PROCEDURE dbo.DeleteDraftRevisions
	@RevisionSourceID bigint
AS
DELETE
  FROM RevisionInformation
 WHERE RevisionSourceID = @RevisionSourceID
   AND Draft = 1
go
-----------
-- PAGES --
-----------

IF OBJECT_ID(N'dbo.StorePage') IS NOT NULL
	DROP PROCEDURE StorePage
go
CREATE PROCEDURE dbo.StorePage
	@PageID bigint,
	@RevisionID bigint,
	@PageName nvarchar(500),
	@PageCode nvarchar(100),
	@ParentPageCode nvarchar(100),
	@TemplateName nvarchar(100),
	@Requestable bit,
	@RequestPath nvarchar(400),
	@ContentType nvarchar(100),
	@PublishDate datetime,
	@ExpiryDate datetime
AS
IF EXISTS (SELECT * FROM Page WHERE PageID=@PageID AND RevisionID=@RevisionID)
	UPDATE Page
	   SET PageName = @PageName,
		   PageCode = @PageCode,
		   ParentPageCode = @ParentPageCode,
		   TemplateName = @TemplateName,
		   Requestable = @Requestable,
		   RequestPath = @RequestPath,
		   ContentType = @ContentType,
		   PublishDate = @PublishDate,
		   ExpiryDate = @ExpiryDate
	 WHERE PageID = @PageID
	   AND RevisionID = @RevisionID
ELSE
	INSERT INTO Page
		(PageID, RevisionID, PageName, PageCode, ParentPageCode, TemplateName, Requestable, RequestPath, ContentType, PublishDate, ExpiryDate)
	VALUES
		(@PageID, @RevisionID, @PageName, @PageCode, @ParentPageCode, @TemplateName, @Requestable, @RequestPath, @ContentType, @PublishDate, @ExpiryDate)
go

IF OBJECT_ID(N'dbo.SelectPageByPageID') IS NOT NULL
	DROP PROCEDURE SelectPageByPageID
go
CREATE PROCEDURE dbo.SelectPageByPageID
	@PageID bigint,
	@ExcludeDraft bit
AS
	SELECT TOP 1 p.*
	  FROM Page p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	 WHERE p.PageID = @PageID
	   AND (@ExcludeDraft = 0 OR (@ExcludeDraft = 1 AND r.Draft = 0))
  ORDER BY r.RevisionDate DESC
go

IF OBJECT_ID(N'dbo.SelectPageByRequestPath') IS NOT NULL
	DROP PROCEDURE SelectPageByRequestPath
go
CREATE PROCEDURE dbo.SelectPageByRequestPath
	@RequestPath nvarchar(400),
	@ExcludeDraft bit
AS
	SELECT p.*
	  FROM Page p
	 WHERE p.RequestPath = @RequestPath
	   AND (getdate()) >= p.PublishDate
	   AND (p.ExpiryDate IS NULL OR p.ExpiryDate >= getdate())
	   AND p.RevisionID = (SELECT TOP 1 ri.RevisionID
							 FROM RevisionInformation ri
							WHERE (@ExcludeDraft = 0 OR (@ExcludeDraft = 1 AND ri.Draft = 0))
							  AND ri.Deleted = 0
							  AND ri.RevisionSourceID = p.PageID
						 ORDER BY RevisionDate DESC)
go

IF OBJECT_ID(N'dbo.ListPages') IS NOT NULL
	DROP PROCEDURE ListPages
go
CREATE PROCEDURE dbo.ListPages
	@Hidden bit,
	@Deleted bit,
	@Draft bit,
	@PageSize bigint,
	@PageNumber bigint,
	@CategorySetName nvarchar(200),
	@CategoryName nvarchar(200),
	@OrderBy int
AS
BEGIN
	DECLARE	@n1 bigint
	DECLARE @n2 bigint
	SET @n1 = @PageSize * (@PageNumber - 1)
	SET @n2 = @n1 + @PageSize - 1;
	
	WITH pageset AS (
		SELECT p.*,
			   COUNT(*) OVER () AS TotalCount,
			   ROW_NUMBER() OVER (ORDER BY PublishDate ASC) AS RowNum_PublishDate_ASC,
			   ROW_NUMBER() OVER (ORDER BY PublishDate DESC) AS RowNum_PublishDate_DESC,
			   ROW_NUMBER() OVER (ORDER BY (NEWID())) AS RowNum_Random
		  FROM Page p
	INNER JOIN RevisionInformation r
			ON p.RevisionID = r.RevisionID
		 WHERE r.RevisionID = (SELECT TOP 1 r2.RevisionID FROM RevisionInformation r2 WHERE r2.RevisionSourceID = p.PageID ORDER BY r2.RevisionDate DESC)
		   AND (@Deleted IS NULL OR (@Deleted = 1 AND r.Deleted = 1) OR (@Deleted = 0 AND r.Deleted = 0))
		   AND (@Draft IS NULL OR (@Draft = 1 AND r.Draft = 1) OR (@Draft = 0 AND r.Draft = 0))
		   AND (@Hidden IS NULL OR (@Hidden = 1 AND r.Hidden = 1) OR (@Hidden = 0 AND r.Hidden = 0))
		   AND (@CategorySetName IS NULL OR @CategoryName IS NULL OR 1 <= (SELECT COUNT(*)
																			 FROM PageCategory c
																			WHERE c.CategoryName = @CategoryName
																			  AND c.CategorySetName = @CategorySetName
																			  AND c.PageRevisionID = p.RevisionID))
	) SELECT *
		FROM pageset
	   WHERE (@OrderBy = 1 AND RowNum_PublishDate_DESC BETWEEN @n1 AND @n1)
		  OR (@OrderBy = 2 AND RowNum_PublishDate_ASC BETWEEN @n1 AND @n1)
		  OR (@OrderBy = 3 AND RowNum_Random BETWEEN @n1 AND @n2)
	ORDER BY CASE @OrderBy
		WHEN 1 THEN RowNum_PublishDate_DESC
		WHEN 2 THEN RowNum_PublishDate_ASC
		ELSE RowNum_Random
	END
END
go
----------------
-- CATEGORIES --
----------------

IF OBJECT_ID(N'dbo.ListCategoriesForPageRevision') IS NOT NULL
	DROP PROCEDURE ListCategoriesForPageRevision
go
CREATE PROCEDURE dbo.ListCategoriesForPageRevision
	@PageRevisionID bigint
AS
	SELECT *
	  FROM PageCategory
	 WHERE PageRevisionID = @PageRevisionID
  ORDER BY CategorySetName, CategoryName
go

IF OBJECT_ID(N'dbo.StorePageCategory') IS NOT NULL
	DROP PROCEDURE StorePageCategory
go
CREATE PROCEDURE dbo.StorePageCategory
	@PageRevisionID bigint,
	@CategorySetName nvarchar(200),
	@CategoryName nvarchar(200)
AS
IF NOT EXISTS (SELECT * FROM PageCategory WHERE PageRevisionID=@PageRevisionID AND CategorySetName=@CategorySetName AND CategoryName=@CategoryName)
	INSERT INTO PageCategory (PageRevisionID, CategorySetName, CategoryName) VALUES (@PageRevisionID, @CategorySetName, @CategoryName);
go

-----------------
-- EDIT FIELDS --
-----------------

IF OBJECT_ID(N'dbo.ListEditFieldsForPageRevision') IS NOT NULL
	DROP PROCEDURE ListEditFieldsForPageRevision
go
CREATE PROCEDURE dbo.ListEditFieldsForPageRevision
	@PageRevisionID bigint
AS
	SELECT *
	  FROM EditFieldInfo
	 WHERE PageRevisionID = @PageRevisionID
  ORDER BY EditFieldTypeIdentifier
go

IF OBJECT_ID(N'dbo.StoreEditFieldInfo') IS NOT NULL
	DROP PROCEDURE StoreEditFieldInfo
go
CREATE PROCEDURE dbo.StoreEditFieldInfo
	@PageRevisionID bigint,
	@EditFieldID bigint,
	@EditFieldTypeIdentifier nvarchar(100),
	@SectionName nvarchar(200),
	@FieldName nvarchar(200),
	@Rank bigint
AS
--# Store EditFieldInfo
INSERT INTO EditFieldInfo (PageRevisionID, EditFieldID, EditFieldTypeIdentifier, SectionName, FieldName, Rank)
VALUES (@PageRevisionID, @EditFieldID, @EditFieldTypeIdentifier, @SectionName, @FieldName, @Rank);
go

IF OBJECT_ID(N'dbo.StoreEditField_TextBox') IS NOT NULL
	DROP PROCEDURE StoreEditField_TextBox
go
CREATE PROCEDURE dbo.StoreEditField_TextBox
	@EditFieldID bigint,
	@Value nvarchar(max)
AS
INSERT INTO EditField_TextBox (EditFieldID, Value) VALUES (@EditFieldID, @Value);
go

IF OBJECT_ID(N'dbo.StoreEditField_Image') IS NOT NULL
	DROP PROCEDURE StoreEditField_Image
go
CREATE PROCEDURE dbo.StoreEditField_Image
	@EditFieldID bigint,
	@SprocketFileID bigint
AS
INSERT INTO EditField_Image (EditFieldID, SprocketFileID) VALUES (@EditFieldID, @SprocketFileID);