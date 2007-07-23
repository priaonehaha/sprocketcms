--# Insert SprocketFile
	INSERT INTO SprocketFile
		(SprocketFileID, ClientSpaceID, FileData, FileTypeExtension, OriginalFileName, ContentType, Title, Description, UploadDate)
	VALUES
		(@SprocketFileID, @ClientSpaceID, @FileData, @FileTypeExtension, @OriginalFileName, @ContentType, @Title, @Description, @UploadDate);

--# Update SprocketFile
		UPDATE SprocketFile SET
			ClientSpaceID = @ClientSpaceID,
			FileData = CASE WHEN @FileData IS NULL THEN FileData ELSE @FileData END,
			FileTypeExtension = @FileTypeExtension,
			OriginalFileName = @OriginalFileName,
			ContentType = @ContentType,
			Title = @Title,
			Description = @Description,
			UploadDate = @UploadDate
		WHERE SprocketFileID = @SprocketFileID;

--# Select SprocketFile
	SELECT SprocketFileID, ClientSpaceID,
			CASE WHEN @GetFileData = 1 THEN FileData ELSE NULL END as [FileData],
			length(FileData) AS [DataLength],
			FileTypeExtension, OriginalFileName, ContentType, Title, Description, UploadDate
	  FROM SprocketFile
	 WHERE SprocketFileID = @SprocketFileID;

--# Delete SprocketFile
	DELETE
	  FROM SprocketFile
	 WHERE SprocketFileID = @SprocketFileID;
