if not exists(select id from sysobjects where name='Clients' and type='U')
CREATE TABLE dbo.Clients
(
	ClientID		uniqueidentifier	PRIMARY KEY,
	Name			nvarchar(100)		NOT NULL,
	Enabled			bit					NOT NULL,
	PrimaryUserID	uniqueidentifier,
	OwnerClientID	uniqueidentifier
)

go

if not exists(select id from sysobjects where name='Users' and type='U')
CREATE TABLE dbo.Users
(
	UserID					uniqueidentifier	PRIMARY KEY,
	ClientID				uniqueidentifier	NOT NULL,
	Username				nvarchar(50)		NOT NULL,
	PasswordHash			nvarchar(200)		NOT NULL,
	FirstName				nvarchar(50),
	Surname					nvarchar(50),
	Email					varchar(100),
	Enabled					bit					NOT NULL,
	Hidden					bit					NOT NULL,
	Locked					bit					NOT NULL,
	Deleted					bit					NOT NULL,
	Activated				bit					NOT NULL,
	ActivationReminderSent	datetime,
	Created					datetime			NOT NULL			
	
	CONSTRAINT UC_Users_UsernameClientID UNIQUE CLUSTERED(ClientID, Username)
)

go

if not exists(select id from sysobjects where name='Roles' and type='U')
CREATE TABLE dbo.Roles
(
	RoleID			uniqueidentifier	PRIMARY KEY,
	RoleCode		nvarchar(100)		NOT NULL,
	ClientID		uniqueidentifier	NOT NULL,
	Name			nvarchar(100)		NOT NULL,
	Enabled			bit					NOT NULL,	-- disables the role without deleting it
	Locked			bit					NOT NULL,	-- prevents role from being modified by users
	Hidden			bit					NOT NULL	-- hides the role from users
	
	CONSTRAINT UC_Roles_RoleCodeClientID UNIQUE CLUSTERED(ClientID, RoleCode)
)

go

if not exists(select id from sysobjects where name='RoleToRole' and type='U')
CREATE TABLE dbo.RoleToRole
(
	RoleID			uniqueidentifier,
	InheritsRoleID	uniqueidentifier
	
	CONSTRAINT PK_RoleToRole PRIMARY KEY CLUSTERED(RoleID, InheritsRoleID)
)

go

if not exists(select id from sysobjects where name='UserToRole' and type='U')
CREATE TABLE dbo.UserToRole
(
	RoleID		uniqueidentifier,
	UserID		uniqueidentifier
	
	CONSTRAINT PK_UserToRole PRIMARY KEY CLUSTERED(RoleID, UserID)
)

go

if not exists(select id from sysobjects where name='PermissionTypes' and type='U')
CREATE TABLE dbo.PermissionTypes
(
	PermissionTypeID		uniqueidentifier	PRIMARY KEY,
	PermissionTypeCode		varchar(100)		UNIQUE NOT NULL,
	Description				nvarchar(1000)		NOT NULL,
	DefaultValue			bit					NOT NULL,
	ModuleRegistrationCode	varchar(100)		NOT NULL
)

go

if not exists(select id from sysobjects where name='Permissions' and type='U')
CREATE TABLE dbo.Permissions
(
	PermissionTypeID	uniqueidentifier,
	OwnerID				uniqueidentifier,				-- relates to both Users and Roles
	Value				bit					NOT NULL
	
	CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED(PermissionTypeID, OwnerID)
)
