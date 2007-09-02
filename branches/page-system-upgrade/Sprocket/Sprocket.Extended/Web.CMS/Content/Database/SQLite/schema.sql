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
*/

CREATE TABLE IF NOT EXISTS RevisionInformation
(
	RevisionID INTEGER PRIMARY KEY,
	RevisionSourceID INTEGER NOT NULL, -- Usually refers to the source table ID. Used to record which revisions relate to which source.
	RevisionDate DATETIME NOT NULL,
	UserID INTEGER NOT NULL,
	Notes TEXT NOT NULL,
	Hidden BOOLEAN NOT NULL, -- indicates that the source content shouldn't be displayed on the public website (i.e. disabled)
	Draft BOOLEAN NOT NULL, -- indicates that this version of the page is a "draft" and thus is not to be used on the public-facing site
	Deleted BOOLEAN NOT NULL -- Any content that is deleted is simply applied with revision.deleted = 1
);

CREATE TABLE IF NOT EXISTS Page
(
	PageID INTEGER NOT NULL,
	PageName TEXT NOT NULL, -- descriptive name for identifying the page purpose easily
	RevisionID INTEGER NOT NULL,
	PageCode TEXT NOT NULL,
	ParentPageCode TEXT NULL,
	TemplateName TEXT NOT NULL,
	Requestable BOOLEAN NOT NULL, -- specifies that RequestPath is to be considered for identifying this page (i.e. that this is a standalone page)
	RequestPath TEXT NOT NULL, -- e.g. 'widgets/yellow/yellow-useful-widget'
	ContentType TEXT NOT NULL, -- e.g. text/html
	PRIMARY KEY (PageID, RevisionID)
);

CREATE TABLE IF NOT EXISTS ContentNode
(
	PageRevisionID INTEGER,
	ContentNodeID INTEGER NOT NULL, -- FK to ContentNode_XXX where XXX = NodeTypeIdentifier
	NodeTypeIdentifier TEXT NOT NULL,
	Name TEXT NOT NULL,
	Rank INTEGER NOT NULL,
	PRIMARY KEY (PageRevisionID, ContentNodeID, NodeTypeIdentifier)
);

CREATE TABLE IF NOT EXISTS ContentNode_TextBox
(
	ContentNodeID INTEGER PRIMARY KEY,
	Value TEXT NOT NULL
);

------------------------------------------------------------------------------------------------------

-- ON DELETE FROM RevisionInformation
DROP TRIGGER IF EXISTS trigger_RevisionInformation_Delete;
CREATE TRIGGER trigger_RevisionInformation_Delete BEFORE DELETE ON RevisionInformation
BEGIN
	DELETE FROM Page WHERE RevisionID = OLD.RevisionID;
END;

-- ON DELETE FROM RevisionInformation
DROP TRIGGER IF EXISTS trigger_Page_Delete;
CREATE TRIGGER trigger_Page_Delete AFTER DELETE ON Page
BEGIN
	DELETE FROM RevisionInformation WHERE RevisionSourceID = OLD.PageID;
	DELETE FROM PageContentNode WHERE PageRevisionID = OLD.RevisionID;
END;

-- ON DELETE FROM Users
DROP TRIGGER IF EXISTS trigger_RevisionInformation_Users_Delete;
CREATE TRIGGER trigger_RevisionInformation_Users_Delete BEFORE DELETE ON Users
BEGIN
	SELECT CASE
		WHEN OLD.UserID IN (SELECT UserID FROM RevisionInformation)
		THEN RAISE(ABORT, 'Error deleting user; they have data attached to their user ID')
	END;
END;
