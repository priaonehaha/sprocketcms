CREATE TABLE IF NOT EXISTS CacheInfo
(
	Identifier TEXT PRIMARY KEY NOT NULL,
	LastAccess DATETIME,
	ExpiryDate DATETIME,
	CreateDate DATETIME,
	PreventExpiryDateChange BOOL,
	SprocketPath TEXT,
	ContentType TEXT,
	Path TEXT
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_SprocketPath ON CacheInfo(SprocketPath);