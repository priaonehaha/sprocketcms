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
		void ExecuteDataScripts();

		IDatabaseInitialiser GetDatabaseInitialiser();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="engine"></param>
		/// <returns></returns>
		bool SelectDatabaseEngine(DatabaseEngine engine);
	}

	public interface IDatabaseInitialiser
	{
		void Execute();
	}
}
