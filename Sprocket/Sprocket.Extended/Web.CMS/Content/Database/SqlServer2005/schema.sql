/*
	Revision Methodology
	--------------------
	- Every save to a page should duplicate the page record and anything relevant that has changed. Individual items related to the
	  page revision are to be retrieved
	- Draft mode revisions are not displayed; use the most recent version where draft = 0
	- If the most recent non-draft revision has Hidden = 1, the content is not to be displayed on the public-facing website
	- When a draft is saved as non-draft, or a draft is cancelled, delete all draft revisions (noone cares about historical drafts)
	- Whenever the page is saved, a new set of ContentNode records are created for the new revision. If the content for an individual
	  ContentNode was changed, a new record with the updated information is stored in the related value table, otherwise the new
	  record points at the existing record in that table.
	- When no template is specified for a page, ALL node types are available and are rendered sequentially to the page
*/

IF OBJECT_ID(N'dbo.RevisionInformation') IS NULL CREATE TABLE dbo.RevisionInformation
(
	RevisionID bigint PRIMARY KEY,
	RevisionSourceID bigint NOT NULL, -- Usually refers to the source table ID. Used to record which revisions relate to which source.
	RevisionDate datetime NOT NULL,
	UserID bigint NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE NO ACTION,
	Notes nvarchar(max) NOT NULL,
	Hidden bit NOT NULL, -- indicates that the source content shouldn't be displayed on the public website (i.e. disabled)
	Draft bit NOT NULL, -- indicates that this version of the page is a "draft" and thus is not to be used on the public-facing site
	Deleted bit NOT NULL -- Any content that is deleted is simply applied with revision.deleted = 1
);

IF OBJECT_ID(N'dbo.Page') IS NULL CREATE TABLE dbo.Page
(
	PageID bigint NOT NULL,
	PageName nvarchar(500) NOT NULL, -- descriptive name for identifying the page purpose easily
	RevisionID bigint NOT NULL,
	PageCode nvarchar(100) NOT NULL,
	ParentPageCode nvarchar(100) NULL,
	TemplateName nvarchar(100) NOT NULL,
	Requestable bit NOT NULL, -- specifies that RequestPath is to be considered for identifying this page (i.e. that this is a standalone page)
	RequestPath nvarchar(400) NOT NULL, -- e.g. 'widgets/yellow/yellow-useful-widget'
	ContentType nvarchar(100) NOT NULL, -- e.g. text/html
	PublishDate datetime NOT NULL, -- this counts as the "date created" as perceived by the public
	ExpiryDate datetime NULL,
	PRIMARY KEY (PageID, RevisionID)
);

IF OBJECT_ID(N'dbo.PageCategory') IS NULL CREATE TABLE dbo.PageCategory
(
	PageRevisionID bigint NOT NULL,
	CategorySetName nvarchar(200) NOT NULL,
	CategoryName nvarchar(200) NOT NULL,
	PRIMARY KEY (PageRevisionID, CategorySetName, CategoryName)
);

IF OBJECT_ID(N'dbo.EditFieldInfo') IS NULL CREATE TABLE dbo.EditFieldInfo -- links a page revision to its edit fields and data and specifies the order in which they appear
(
	PageRevisionID bigint,
	EditFieldID bigint NOT NULL, -- FK to EditField_XXX where XXX = EditFieldTypeIdentifier
	EditFieldTypeIdentifier nvarchar(100) NOT NULL, -- e.g. 'TextBox'
	SectionName nvarchar(200) NOT NULL, -- identifies the content field in which the node appears
	FieldName nvarchar(200) NOT NULL,
	Rank bigint NOT NULL,
	PRIMARY KEY (PageRevisionID, EditFieldID, EditFieldTypeIdentifier)
);

IF OBJECT_ID(N'dbo.EditField_TextBox') IS NULL CREATE TABLE dbo.EditField_TextBox
(
	EditFieldID bigint PRIMARY KEY,
	Value nvarchar(max) NOT NULL
);

IF OBJECT_ID(N'dbo.EditField_Image') IS NULL CREATE TABLE dbo.EditField_Image
(
	EditFieldID bigint,
	SprocketFileID bigint,
	PRIMARY KEY (EditFieldID, SprocketFileID)
);
go
------------------------------------------------------------------------------------------------------

-- ON DELETE FROM RevisionInformation
IF OBJECT_ID(N'trigger_RevisionInformation_OnDelete') IS NOT NULL
	DROP TRIGGER trigger_RevisionInformation_OnDelete
go
CREATE TRIGGER trigger_RevisionInformation_OnDelete ON dbo.RevisionInformation AFTER DELETE
AS
BEGIN
	DELETE FROM Page WHERE RevisionID IN (SELECT RevisionID FROM deleted);
	DELETE FROM PageCategory WHERE PageRevisionID IN (SELECT RevisionID FROM deleted);
	DELETE FROM EditFieldInfo WHERE PageRevisionID IN (SELECT RevisionID FROM deleted);
END
go

-- ON DELETE FROM Page
IF OBJECT_ID(N'trigger_Page_OnDelete') IS NOT NULL
	DROP TRIGGER trigger_Page_OnDelete
go
CREATE TRIGGER trigger_Page_OnDelete ON dbo.Page AFTER DELETE
AS
BEGIN
	DELETE FROM RevisionInformation WHERE RevisionID IN (SELECT RevisionID FROM deleted);
	DELETE FROM EditFieldInfo WHERE PageRevisionID IN (SELECT RevisionID FROM deleted);
END
go

-- ON DELETE FROM EditField
IF OBJECT_ID(N'trigger_EditField_OnDelete') IS NOT NULL
	DROP TRIGGER trigger_EditField_OnDelete
go
CREATE TRIGGER trigger_EditField_OnDelete ON dbo.EditFieldInfo AFTER DELETE
AS
BEGIN
	DELETE FROM EditField_TextBox WHERE EditFieldID NOT IN (SELECT EditFieldID FROM EditFieldInfo);
	DELETE FROM EditField_Image WHERE EditFieldID NOT IN (SELECT EditFieldID FROM EditFieldInfo);
END
go

-- ON DELETE FROM EditField_TextBox
IF OBJECT_ID(N'trigger_EditField_TextBox_OnDelete') IS NOT NULL
	DROP TRIGGER trigger_EditField_TextBox_OnDelete
go
CREATE TRIGGER trigger_EditField_TextBox_OnDelete ON dbo.EditField_TextBox AFTER DELETE
AS
BEGIN
	DELETE FROM EditFieldInfo WHERE EditFieldID IN (SELECT EditFieldID FROM deleted)
		AND PageRevisionID NOT IN (SELECT RevisionID FROM RevisionInformation);
END
go

-- ON DELETE FROM EditField_Image
IF OBJECT_ID(N'trigger_EditField_TextBox_OnDelete') IS NOT NULL
	DROP TRIGGER trigger_EditField_TextBox_OnDelete
go
CREATE TRIGGER trigger_EditField_TextBox_OnDelete ON dbo.EditField_Image AFTER DELETE
AS
BEGIN
	DELETE FROM EditFieldInfo WHERE EditFieldID IN (SELECT EditFieldID FROM deleted)
		AND PageRevisionID NOT IN (SELECT RevisionID FROM RevisionInformation)
	DELETE FROM SprocketFile WHERE SprocketFileID IN (SELECT EditFieldID FROM deleted)
END
