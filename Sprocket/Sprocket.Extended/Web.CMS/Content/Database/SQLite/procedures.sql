---------------
-- REVISIONS --
---------------

--# Store RevisionInformation
-- there is never a reason to change any of the existing fields
INSERT INTO RevisionInformation
	(RevisionID, RevisionSourceID, RevisionDate, UserID, Notes, Hidden, Draft, Deleted)
VALUES
	(@RevisionID, @RevisionSourceID, @RevisionDate, @UserID, @Notes, @Hidden, @Draft, @Deleted);

--# Select RevisionInformation
SELECT *
  FROM RevisionInformation
 WHERE RevisionID = @RevisionID

--# Delete Revision By RevisionID
DELETE FROM RevisionInformation WHERE RevisionID = @RevisionID;

--# List Revisions By Source
	SELECT *
	  FROM RevisionInformation
	 WHERE RevisionSourceID = @RevisionSourceID
  ORDER BY RevisionDate DESC

-----------
-- PAGES --
-----------

--# Store Page
INSERT OR REPLACE INTO Page
	(PageID, RevisionID, PageName, PageCode, ParentPageCode, TemplateName, Requestable, RequestPath, ContentType)
VALUES
	(@PageID, @RevisionID, @PageName, @PageCode, @ParentPageCode, @TemplateName, @Requestable, @RequestPath, @ContentType)

--# Select Page By PageID
	SELECT p.*
	  FROM Page p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	 WHERE p.PageID = @PageID
	   AND (@ExcludeDraft = 0 OR (@ExcludeDraft = 1 AND r.Draft = 0))
  ORDER BY r.RevisionDate DESC
     LIMIT 1

--# Select Page By RequestPath
	SELECT p.*
	  FROM Page p
	 WHERE p.RequestPath LIKE @RequestPath
	   AND p.RevisionID IN (SELECT ri.RevisionID
							  FROM RevisionInformation ri
							 WHERE (@ExcludeDraft = 0 OR (@ExcludeDraft = 1 AND ri.Draft = 0))
							   AND ri.Deleted = 0
							   AND ri.RevisionSourceID = p.PageID
						  ORDER BY RevisionDate DESC
							 LIMIT 1)

--# List Pages
	SELECT p.*
	  FROM Page p
INNER JOIN RevisionInformation r
		ON p.RevisionID = r.RevisionID
	   AND r.RevisionID = (SELECT r2.RevisionID FROM RevisionInformation r2 WHERE r2.RevisionSourceID = p.PageID ORDER BY r2.RevisionDate DESC)
	   AND r.Deleted = 0
  ORDER BY r.RevisionDate DESC

--# List ContentNodes
	SELECT *
	  FROM ContentNode
	 WHERE PageRevisionID = @PageRevisionID
  ORDER BY NodeTypeIdentifier

-----------------
-- EDIT FIELDS --
-----------------

--# List EditFields For Page Revision
	SELECT *
	  FROM EditFieldInfo
	 WHERE PageRevisionID = @PageRevisionID
  ORDER BY EditFieldTypeIdentifier

--# Store EditFieldInfo
INSERT OR REPLACE INTO EditFieldInfo (PageRevisionID, EditFieldID, EditFieldTypeIdentifier, SectionName, FieldName, Rank)
VALUES (@PageRevisionID, @EditFieldID, @EditFieldTypeIdentifier, @SectionName, @FieldName, @Rank);

--# Store EditField_TextBox
INSERT OR REPLACE INTO EditField_TextBox (EditFieldID, Value) VALUES (@EditFieldID, @Value);

--# Store EditField_Image
INSERT OR REPLACE INTO EditField_Image (EditFieldID, SprocketFileID) VALUES (@EditFieldID, @SprocketFileID);