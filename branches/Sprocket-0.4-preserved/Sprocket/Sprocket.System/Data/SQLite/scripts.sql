--# Schema
CREATE TABLE IF NOT EXISTS UniqueIDMaker (
	NextID integer primary key
);

--# GetUniqueID
INSERT INTO UniqueIDMaker (NextID) VALUES (null);
DELETE FROM UniqueIDMaker WHERE NextID < last_insert_rowid() - 5;
SELECT last_insert_rowid();