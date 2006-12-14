IF OBJECT_ID(N'dbo.SnapCanvas') IS NULL
CREATE TABLE dbo.SnapCanvas
(
	SnapCanvasID bigint PRIMARY KEY,
	UnitWidth smallint NOT NULL,
	UnitHeight smallint NOT NULL,
	WidthExpandable bit NOT NULL,
	HeightExpandable bit NOT NULL,
	UnitSize smallint NOT NULL,
	GapSize smallint NOT NULL
)

IF OBJECT_ID(N'dbo.SnapPanel') IS NULL
CREATE TABLE dbo.SnapPanel
(
	SnapPanelID bigint PRIMARY KEY,
	SnapCanvasID bigint NOT NULL FOREIGN KEY REFERENCES SnapCanvas(SnapCanvasID) ON DELETE CASCADE,
	WidgetTypeID nvarchar(100) NOT NULL,
	UnitWidth smallint NOT NULL,
	UnitHeight smallint NOT NULL,
	UnitX smallint NOT NULL,
	UnitY smallint NOT NULL,
	MaxUnitWidth smallint NOT NULL,
	MinUnitWidth smallint NOT NULL,
	MaxUnitHeight smallint NOT NULL,
	MinUnitHeight smallint NOT NULL,
	LockPosition bit NOT NULL,
	LockSize bit NOT NULL,
	AllowDelete bit NOT NULL,
	AllowEdit bit NOT NULL
)
go

--------------------------------------------------------------------------------------------------------------

IF OBJECT_ID(N'dbo.SnapCanvas_Store') IS NOT NULL
	DROP PROCEDURE SnapCanvas_Store
go
CREATE PROCEDURE dbo.SnapCanvas_Store
	@SnapCanvasID bigint OUTPUT,
	@UnitWidth smallint,
	@UnitHeight smallint,
	@WidthExpandable bit,
	@HeightExpandable bit,
	@UnitSize smallint,
	@GapSize smallint
AS
BEGIN
	IF NOT EXISTS (SELECT SnapCanvasID FROM SnapCanvas WHERE SnapCanvasID = @SnapCanvasID)
	BEGIN
		IF @SnapCanvasID = 0 OR @SnapCanvasID IS NULL
			EXEC GetUniqueID @SnapCanvasID OUTPUT
		INSERT INTO SnapCanvas
			(SnapCanvasID, UnitWidth, UnitHeight, WidthExpandable, HeightExpandable, UnitSize, GapSize)
		VALUES
			(@SnapCanvasID, @UnitWidth, @UnitHeight, @WidthExpandable, @HeightExpandable, @UnitSize, @GapSize)
	END
	ELSE
	BEGIN
		UPDATE SnapCanvas SET
			UnitWidth = @UnitWidth,
			UnitHeight = @UnitHeight,
			WidthExpandable = @WidthExpandable,
			HeightExpandable = @HeightExpandable,
			UnitSize = @UnitSize,
			GapSize = @GapSize
		WHERE SnapCanvasID = @SnapCanvasID
	END
END
go

IF OBJECT_ID(N'dbo.SnapCanvas_Select') IS NOT NULL
	DROP PROCEDURE SnapCanvas_Select
go
CREATE PROCEDURE dbo.SnapCanvas_Select
	@SnapCanvasID bigint
AS
BEGIN
	SELECT *
	  FROM SnapCanvas
	 WHERE SnapCanvasID = @SnapCanvasID
END

go

IF OBJECT_ID(N'dbo.SnapCanvas_Delete') IS NOT NULL
	DROP PROCEDURE SnapCanvas_Delete
go
CREATE PROCEDURE dbo.SnapCanvas_Delete
	@SnapCanvasID bigint
AS
BEGIN
	DELETE
	  FROM SnapCanvas
	 WHERE SnapCanvasID = @SnapCanvasID
END

--------------------------------------------------------------------------------------------------------------
go

IF OBJECT_ID(N'dbo.SnapPanel_Store') IS NOT NULL
	DROP PROCEDURE SnapPanel_Store
go
CREATE PROCEDURE dbo.SnapPanel_Store
	@SnapPanelID bigint OUTPUT,
	@SnapCanvasID bigint,
	@WidgetTypeID nvarchar(100),
	@UnitWidth smallint,
	@UnitHeight smallint,
	@UnitX smallint,
	@UnitY smallint,
	@MaxUnitWidth smallint,
	@MinUnitWidth smallint,
	@MaxUnitHeight smallint,
	@MinUnitHeight smallint,
	@LockPosition bit,
	@LockSize bit,
	@AllowDelete bit,
	@AllowEdit bit
AS
BEGIN
	IF @SnapPanelID = 0 OR NOT EXISTS (SELECT SnapPanelID FROM SnapPanel WHERE SnapPanelID = @SnapPanelID)
	BEGIN
		IF @SnapPanelID = 0 OR @SnapPanelID IS NULL
			EXEC GetUniqueID @SnapPanelID OUTPUT
		INSERT INTO SnapPanel
			(SnapPanelID, SnapCanvasID, WidgetTypeID, UnitWidth, UnitHeight, UnitX, UnitY, MaxUnitWidth, MinUnitWidth, MaxUnitHeight, MinUnitHeight, LockPosition, LockSize, AllowDelete, AllowEdit)
		VALUES
			(@SnapPanelID, @SnapCanvasID, @WidgetTypeID, @UnitWidth, @UnitHeight, @UnitX, @UnitY, @MaxUnitWidth, @MinUnitWidth, @MaxUnitHeight, @MinUnitHeight, @LockPosition, @LockSize, @AllowDelete, @AllowEdit)
	END
	ELSE
	BEGIN
		UPDATE SnapPanel SET
			SnapCanvasID = @SnapCanvasID,
			WidgetTypeID = @WidgetTypeID,
			UnitWidth = @UnitWidth,
			UnitHeight = @UnitHeight,
			UnitX = @UnitX,
			UnitY = @UnitY,
			MaxUnitWidth = @MaxUnitWidth,
			MinUnitWidth = @MinUnitWidth,
			MaxUnitHeight = @MaxUnitHeight,
			MinUnitHeight = @MinUnitHeight,
			LockPosition = @LockPosition,
			LockSize = @LockSize,
			AllowDelete = @AllowDelete,
			AllowEdit = @AllowEdit
		WHERE SnapPanelID = @SnapPanelID
	END
END
go

IF OBJECT_ID(N'dbo.SnapPanel_Select') IS NOT NULL
	DROP PROCEDURE SnapPanel_Select
go
CREATE PROCEDURE dbo.SnapPanel_Select
	@SnapPanelID bigint
AS
BEGIN
	SELECT *
	  FROM SnapPanel
	 WHERE SnapPanelID = @SnapPanelID
END

go

IF OBJECT_ID(N'dbo.SnapPanel_Delete') IS NOT NULL
	DROP PROCEDURE SnapPanel_Delete
go
CREATE PROCEDURE dbo.SnapPanel_Delete
	@SnapPanelID bigint
AS
BEGIN
	DELETE
	  FROM SnapPanel
	 WHERE SnapPanelID = @SnapPanelID
END
go
--------------------------------------------------------------------------------------------------------------

IF OBJECT_ID(N'dbo.SnapCanvas_ListPanels') IS NOT NULL
	DROP PROCEDURE SnapCanvas_ListPanels
go
CREATE PROCEDURE dbo.SnapCanvas_ListPanels
	@SnapCanvasID bigint
AS
	SELECT *
	  FROM SnapPanel
	 WHERE SnapCanvasID = @SnapCanvasID
go
