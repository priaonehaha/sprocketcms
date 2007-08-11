UPDATE CacheInfo
   SET LastAccess = @LastAccess,
	   ExpiryDate = @ExpiryDate
 WHERE Identifier = @Identifier