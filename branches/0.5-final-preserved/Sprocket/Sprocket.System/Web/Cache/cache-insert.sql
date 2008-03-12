INSERT OR REPLACE INTO CacheInfo
(Identifier, LastAccess, ExpiryDate, CreateDate, PreventExpiryDateChange, SprocketPath, ContentType, Path)
VALUES
(@Identifier, @LastAccess, @ExpiryDate, @CreateDate, @PreventExpiryDateChange, @SprocketPath, @ContentType, @Path)
