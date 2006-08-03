if not exists(select id from sysobjects where name='SprocketFiles' and type='U')
create table dbo.SprocketFiles
(
	SprocketFileID		uniqueidentifier PRIMARY KEY,
	FileTypeExtension	nvarchar(200),		-- e.g. zip, txt or jpg
	SprocketPath		nvarchar(200),		-- the request path e.g. files/images/fluff.jpg or admin/hidden0023
	ContentType			nvarchar(200),		-- the request path e.g. files/images/fluff.jpg or admin/hidden0023
	ParentFileID		uniqueidentifier,	-- removing/changing the parent file removes/changes this file also
	OwnerID				uniqueidentifier,	-- the entity that claims ownership over the file
	ClientID			uniqueidentifier,	-- the client website that owns the file
	CategoryCode		nvarchar(100),		-- an optional code to help modules categorise files
	ModuleRegCode		varchar(100),		-- the module that is handles control of this file
	Description			ntext,				-- an optional description for the file,
	UploadDate			datetime			-- the date/time the file was uploaded
)
