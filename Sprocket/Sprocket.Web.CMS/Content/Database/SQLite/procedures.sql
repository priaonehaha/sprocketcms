---------------
-- REVISIONS --
---------------

--# Insert Revision
-- there is never a reason to change any of the existing fields (other than IsCurrent)
INSERT OR REPLACE INTO RevisionInformation
	(RevisionID, RevisionSourceID, RevisionDate, UserID, Notes, Hidden, Draft, Deleted)
VALUES
	(@RevisionID, @RevisionSourceID, @RevisionDate, @UserID, @Notes, @Hidden, @Draft, @Deleted);

--# Delete Revision By RevisionID
DELETE FROM RevisionInformation WHERE RevisionID = @RevisionID;

-----------
-- PAGES --
-----------

--# Store Page
INSERT OR REPLACE INTO Pages (PageID, RevisionID, PageCode, ParentPageCode, TemplateName, RequestPath, ContentType)
VALUES (@PageID, @RevisionID, @PageCode, @ParentPageCode, @TemplateName, @RequestPath, @ContentType)

--# Select Page By PageID
	SELECT p.*
	  FROM Pages p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	 WHERE p.PageID = @PageID
	   AND (@ExcludeDraft = 0 OR (@ExcludeDraft = 1 AND r.Draft = 0))
  ORDER BY r.RevisionDate DESC
     LIMIT 1

--# Select Page By RequestPath
	SELECT p.*
	  FROM Pages p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	 WHERE p.RequestPath LIKE @RequestPath
	   AND r.Deleted = 0
	   AND (@ExcludeDraft = 0 OR (@ExcludeDraft = 1 AND r.Draft = 0))
  ORDER BY r.RevisionDate DESC
	 LIMIT 1

--# List Pages
	SELECT p.*
	  FROM Pages p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	   AND r.RevisionID = (SELECT r2.RevisionID FROM RevisionInformation r2 WHERE r2.RevisionSourceID = p.PageID ORDER BY r2.RevisionDate DESC)
  ORDER BY r.RevisionDate DESC
