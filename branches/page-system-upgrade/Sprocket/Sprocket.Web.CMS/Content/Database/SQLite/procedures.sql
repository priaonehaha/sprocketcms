---------------
-- REVISIONS --
---------------

--# Insert Revision
-- there is never a reason to change any of the existing fields (other than IsCurrent)
UPDATE RevisionInformation SET IsCurrent = 0 WHERE RevisionGroupID = @RevisionGroupID;
INSERT INTO RevisionInformation
	(RevisionID, RevisionGroupID, RevisionDate, IsCurrent, UserID, Notes, Deleted)
VALUES
	(@RevisionID, @RevisionGroupID, @RevisionDate, 1, @UserID, @Notes, @Deleted);

--# Delete Revision By RevisionID
DELETE FROM RevisionInformation WHERE RevisionID = @RevisionID;

--# List Recent Revisions
select *
from revisioninformation r
where Notes <> ''
and r.IsCurrent = 1
order by RevisionDate desc
limit @limit

-----------
-- PAGES --
-----------

--# Store Page
INSERT OR REPLACE INTO Pages (PageID, RevisionID, PageCode, ParentPageCode, TemplateName, RequestPath, ContentType)
VALUES (@PageID, @RevisionID, @PageCode, @ParentPageCode, @TemplateName, @RequestPath, @ContentType)

--# Select Page By ID
	SELECT p.*
	  FROM Pages p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	   AND r.IsCurrent = 1
	 WHERE p.PageID = @PageID

--# Select Page By RequestPath
	SELECT p.*
	  FROM Pages p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	   AND r.IsCurrent = 1
	 WHERE p.RequestPath LIKE @RequestPath
	 LIMIT 1
