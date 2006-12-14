using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.FileManager
{
	public interface IFileManagerDataLayer
	{
		Type DatabaseHandlerType { get; }
		Result InitialiseDatabase();

		#region Definitions for SprocketFile

		event InterruptableEventHandler<SprocketFile> OnBeforeDeleteSprocketFile;
		event NotificationEventHandler<SprocketFile> OnSprocketFileDeleted;
		Result Store(SprocketFile sprocketFile);
		Result Delete(SprocketFile sprocketFile);
		SprocketFile SelectSprocketFile(long id, bool getFileData);

		#endregion
	}
}
