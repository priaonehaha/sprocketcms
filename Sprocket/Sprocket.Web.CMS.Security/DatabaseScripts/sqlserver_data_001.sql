--==================--
-- VERSION 1.0 DATA --
--==================--

IF NOT EXISTS(SELECT * FROM PermissionTypes WHERE PermissionTypeCode = 'ACCESS_ADMIN')
BEGIN
	-- only one of each of these values should exist in the database, so
	-- wrap calls in a transaction in case two websites try at the same time
	-- to create the permission types, causing one to throw a unique
	-- constraint violation exception
	BEGIN TRANSACTION
	DECLARE @permID uniqueidentifier
	SELECT @permID = NEWID()
	EXEC dbo.CreatePermissionType @permID, 'ACCESS_ADMIN', 'Allow access to website administration', 0, 'ContentManager'
	IF @@ERROR <> 0
		ROLLBACK TRANSACTION
	ELSE
		COMMIT TRANSACTION
END