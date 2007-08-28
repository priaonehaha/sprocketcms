CREATE TABLE IF NOT EXISTS RevisionInformation
(
	RevisionID INTEGER PRIMARY KEY,
	RevisionGroupID INTEGER NOT NULL, -- Usually refers to the source table ID. Used to record which revisions relate to which source.
	RevisionDate DATETIME NOT NULL,
	IsCurrent BOOLEAN NOT NULL,
	UserID INTEGER NOT NULL,
	Notes TEXT,
	Deleted BOOLEAN NOT NULL -- Any content that is deleted is simply applied with revision.deleted = 1
);

CREATE TABLE IF NOT EXISTS Pages
(
	PageID INTEGER NOT NULL,
	RevisionID INTEGER NOT NULL,
	PageCode TEXT UNIQUE NOT NULL,
	ParentPageCode TEXT NULL,
	TemplateName TEXT NULL,
	RequestPath TEXT NULL,
	ContentType TEXT NULL,
	PRIMARY KEY (PageID, RevisionID)
);

------------------------------------------------------------------------------------------------------

DROP TRIGGER IF EXISTS trigger_RevisionInformation_Delete;
CREATE TRIGGER trigger_RevisionInformation_Delete BEFORE DELETE ON RevisionInformation
BEGIN
	DELETE FROM Pages WHERE RevisionID = OLD.RevisionID;
	
	UPDATE RevisionInformation -- set the latest revision to current
	   SET IsCurrent = 1
	 WHERE RevisionID = (SELECT RevisionID
						   FROM RevisionInformation
						  WHERE RevisionGroupID = OLD.RevisionGroupID
					   ORDER BY RevisionDate DESC
						 LIMIT 1);
END;

DROP TRIGGER IF EXISTS trigger_RevisionInformation_Users_Delete;
CREATE TRIGGER trigger_RevisionInformation_Users_Delete BEFORE DELETE ON Users
BEGIN
	SELECT CASE
		WHEN OLD.UserID IN (SELECT UserID FROM RevisionInformation)
		THEN RAISE(ABORT, 'Error deleting user; they have data attached to their user ID')
	END;
END;
