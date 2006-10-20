CREATE TABLE IF NOT EXISTS ClientSpaces
(
	ClientSpaceID			GUID				PRIMARY KEY,
	Name					TEXT(100)			NOT NULL,
	Enabled					BOOLEAN				NOT NULL,
	PrimaryUserID			INTEGER,
	OwnerClientSpaceID	GUID
);

CREATE TABLE IF NOT EXISTS Users
(
	UserID					INTEGER				PRIMARY KEY,
	ClientSpaceID			GUID				NOT NULL,
	Username				TEXT(50)			NOT NULL,
	PasswordHash			TEXT(200)			NOT NULL,
	FirstName				TEXT(50),
	Surname					TEXT(50),
	Email					TEXT(100),
	Enabled					BOOLEAN				NOT NULL,
	Hidden					BOOLEAN				NOT NULL,
	Locked					BOOLEAN				NOT NULL,
	Deleted					BOOLEAN				NOT NULL,
	Activated				BOOLEAN				NOT NULL,
	ActivationReminderSent	DATETIME,
	Created					DATETIME			NOT NULL,
	
	CONSTRAINT UNIQUE (ClientSpaceID, Username) ON CONFLICT FAIL
);

CREATE TABLE IF NOT EXISTS Roles
(
	RoleID					INTEGER				PRIMARY KEY,
	RoleCode				TEXT(100)			NOT NULL,
	ClientSpaceID			GUID				NOT NULL,
	Name					TEXT(100)			NOT NULL,
	Enabled					BOOLEAN				NOT NULL,	-- disables the role without deleting it
	Locked					BOOLEAN				NOT NULL,	-- prevents role from being modified by users
	Hidden					BOOLEAN				NOT NULL,	-- hides the role from users
	
	CONSTRAINT UNIQUE (ClientSpaceID, RoleCode) ON CONFLICT FAIL
);

CREATE TABLE IF NOT EXISTS RoleToRole
(
	RoleID					INTEGER,
	InheritsRoleID			INTEGER,
	
	CONSTRAINT PRIMARY KEY (RoleID, InheritsRoleID) ON CONFLICT FAIL
);

CREATE TABLE IF NOT EXISTS UserToRole
(
	RoleID					INTEGER,
	UserID					INTEGER,
	
	CONSTRAINT PRIMARY KEY (RoleID, UserID) ON CONFLICT FAIL
);

CREATE TABLE IF NOT EXISTS PermissionTypes
(
	PermissionTypeID		INTEGER				PRIMARY KEY,
	PermissionTypeCode		TEXT(100)			UNIQUE NOT NULL,
	Description				TEXT(1000)			NOT NULL,
	DefaultValue			BOOLEAN				NOT NULL
);

CREATE TABLE IF NOT EXISTS Permissions
(
	PermissionTypeID		INTEGER				NOT NULL,
	RoleID					INTEGER,			-- One of either RoleID or UserID must have a value, but not both
	UserID					INTEGER,
	Value					BOOLEAN				NOT NULL,
	
	CONSTRAINT PRIMARY KEY (PermissionTypeID, OwnerID) ON CONFLICT FAIL
);
