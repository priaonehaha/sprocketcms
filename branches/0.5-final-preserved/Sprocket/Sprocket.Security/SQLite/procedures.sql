--# Insert First Client
INSERT OR IGNORE INTO ClientSpaces (ClientSpaceID, Name, Enabled, PrimaryUserID)
VALUES (@ClientSpaceID, 'SprocketCMS', 1, NULL);

--# Authenticate
	SELECT u.Enabled
	  FROM Users AS u
INNER JOIN ClientSpaces AS c
		ON u.ClientSpaceID = c.ClientSpaceID
	 WHERE u.Username = @Username
	   AND u.PasswordHash = @PasswordHash
	   AND u.Enabled = 1
	   AND c.Enabled = 1
	   AND c.ClientSpaceID = @ClientSpaceID;

--# IsUsernameTaken
	SELECT COUNT(*)
	  FROM Users AS [u]
INNER JOIN ClientSpaces AS [c]
		ON u.ClientSpaceID = c.ClientSpaceID
	 WHERE u.Username = @Username
	   AND c.ClientSpaceID = @ClientSpaceID
	   AND u.UserID <> @ExcludeUserID

--# IsEmailAddressTaken
	SELECT COUNT(*)
	  FROM Users AS u
INNER JOIN ClientSpaces AS c
		ON u.ClientSpaceID = c.ClientSpaceID
	 WHERE u.Email = @Email
	   AND c.ClientSpaceID = @ClientSpaceID
	   AND u.UserID <> @ExcludeUserID

--# Store ClientSpace
INSERT OR REPLACE INTO ClientSpaces (ClientSpaceID, Name, Enabled, PrimaryUserID, OwnerClientSpaceID)
VALUES (@ClientSpaceID, @Name, @Enabled, @PrimaryUserID, @OwnerClientSpaceID);

--# Store PermissionType
INSERT OR REPLACE INTO PermissionTypes
(PermissionTypeID, PermissionTypeCode, Description, DefaultValue)
VALUES
(@PermissionTypeID, @PermissionTypeCode, @Description, @DefaultValue)

--# Store Role
INSERT OR REPLACE INTO Roles
(RoleID, RoleCode, ClientSpaceID, Name, Enabled, Locked, Hidden)
VALUES
(@RoleID, @RoleCode, @ClientSpaceID, @Name, @Enabled, @Locked, @Hidden)

--# Store User
INSERT OR REPLACE INTO Users
(UserID, ClientSpaceID, Username, PasswordHash, FirstName, Surname, Email, LocalTimeOffsetHours,
 Enabled, Hidden, Locked, Deleted, Activated, ActivationReminderSent, Created)
VALUES
(@UserID, @ClientSpaceID, @Username, @PasswordHash, @FirstName, @Surname, @Email, @LocalTimeOffsetHours,
 @Enabled, @Hidden, @Locked, @Deleted, @Activated, @ActivationReminderSent, @Created);
 
--# AssignPermissionToUser
 INSERT OR REPLACE INTO Permissions
 (PermissionTypeID, RoleID, UserID, Value)
 SELECT PermissionTypeID, NULL, @UserID, 1
   FROM PermissionTypes
  WHERE PermissionTypeCode = @PermissionTypeCode
  
--# Select User By Username
	SELECT *
	  FROM Users
	 WHERE Username = @Username
	   AND ClientSpaceID = @ClientSpaceID
  
--# Select User By UserID
	SELECT *
	  FROM Users
	 WHERE UserID = @UserID

--# Select ClientSpace
	SELECT *
	  FROM ClientSpaces
	 WHERE ClientSpaceID = @ClientSpaceID

--# ListPermissionsAndRolesForUser
	SELECT * FROM PermissionTypes;
	SELECT * FROM Roles;
	SELECT * FROM RoleToRole;
	SELECT * FROM Permissions WHERE RoleID IS NOT NULL;
	SELECT * FROM Permissions WHERE UserID = @UserID;
	SELECT * FROM UserToRole WHERE UserID = @UserID;

--# Filter Users
	SELECT *
	  FROM Users
	 WHERE	(@ClientSpaceID IS NULL OR ClientSpaceID = @ClientSpaceID)
		AND (@Username IS NULL OR (Username LIKE '%' || @Username || '%') OR @Username = Username )
		AND (@FirstName IS NULL OR (FirstName LIKE '%' || @FirstName || '%') OR @FirstName = FirstName)
		AND (@Surname IS NULL OR (Surname LIKE '%' || @Surname || '%') OR @Surname = Surname)
		AND (@Email IS NULL OR (Email LIKE '%' || @Email || '%') OR @Email = Email)
		AND (Deleted IS NULL OR Deleted = 0)
		AND (@Activated IS NULL OR Activated = @Activated)
	ORDER BY Username
	[limit]

--# ListAllRolesAgainstUser
	SELECT r.*, CASE WHEN ur.RoleID IS NULL THEN 0 ELSE 1 END AS HasRole
	  FROM Roles r
 LEFT JOIN UserToRole ur
		ON ur.RoleID = r.RoleID
	   AND ur.UserID = @UserID
  ORDER BY r.Name
