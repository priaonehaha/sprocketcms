using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Data
{
	public interface IDataHandlerModule
	{
		/// <summary>
		/// This method is executed whenever the application starts. Any data verification
		/// tasks should be performed at this point, for example, making sure that certain
		/// necessary records exist. This is especially important when the database structure
		/// has just been upgraded and requires additional or altered data to be entered into
		/// any new or altered tables.
		/// </summary>
		/// <param name="engine">The database engine being used.</param>
		void ExecuteDataScripts(DatabaseEngine engine);
		/// <summary>
		/// This method is only called when the module is to be removed. It should remove any
		/// database objects used by the module.
		/// </summary>
		/// <param name="engine">The database engine being used.</param>
		void DeleteDatabaseStructure(DatabaseEngine engine);
		/// <summary>
		/// This should return a value indicating whether or not the module is compatible with the
		/// specified database engine.
		/// </summary>
		/// <param name="engine">The database engine being used.</param>
		/// <returns>True or false</returns>
		bool SupportsDatabaseEngine(DatabaseEngine engine);
	}
}
