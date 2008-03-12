CREATE TABLE IF NOT EXISTS SprocketFile
(
	SprocketFileID		INTEGER PRIMARY KEY,
	ClientSpaceID		INTEGER NOT NULL,		-- the client website that owns the file
	FileData			BLOB NOT NULL,			-- the raw image data
	FileTypeExtension	TEXT NOT NULL,			-- e.g. zip, txt or jpg
	OriginalFileName	TEXT NOT NULL,			-- the original uploaded filename
	ContentType			TEXT NOT NULL,			-- the uploaded content MIME type
	Title				TEXT NOT NULL,			-- an optional title for the file
	Description			TEXT NOT NULL,			-- an optional description for the file,
	UploadDate			DATETIME NOT NULL		-- the date/time the file was uploaded
);