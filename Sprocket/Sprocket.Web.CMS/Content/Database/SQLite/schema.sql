/*
	Revision Methodology
	--------------------
	- Every save to a page should duplicate the page record and anything relevant that has changed. Individual items related to the
	  page revision are to be retrieved
	- Draft mode revisions are not displayed; use the most recent version where draft = 0
	- If the most recent non-draft revision has Hidden = 1, the content is not to be displayed on the public-facing website
	- When a draft is saved as non-draft, or a draft is cancelled, delete all draft revisions (noone cares about historical drafts)
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
	RevisionID INTEGER NOT NULL,
	PageCode TEXT UNIQUE NOT NULL,
	ParentPageCode INTEGER NOT NULL,
	TemplateName TEXT NOT NULL,
	Requestable BOOLEAN NOT NULL, -- specifies that RequestPath is to be considered for identifying this page (i.e. that this is a standalone page)
	RequestPath TEXT NOT NULL, -- e.g. 'widgets/yellow/yellow-useful-widget'
	ContentType TEXT NOT NULL, -- e.g. text/html
	PRIMARY KEY (PageID, RevisionID)
);

------------------------------------------------------------------------------------------------------

-- ON DELETE FROM RevisionInformation
DROP TRIGGER IF EXISTS trigger_RevisionInformation_Delete;
CREATE TRIGGER trigger_RevisionInformation_Delete BEFORE DELETE ON RevisionInformation
BEGIN
	DELETE FROM Pages WHERE RevisionID = OLD.RevisionID;
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
