using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Sprocket.Data
{
	public interface IDatabaseHandler
	{
		/// <summary>
		/// Create tables, check that required data exists, etc.
		/// </summary>
		/// <returns>A <see cref="Result" /> indicating success or
		/// failure</returns>
		Result Initialise();

		/// <summary>
		/// Check configuration settings, if any, and return a result
		/// indicating whether the database has enough information to
		/// successfully call <see cref="Initialise"/>.
		/// </summary>
		/// <returns>A <see cref="Result"/> value indicating if the
		/// <c cref="Initialise" /> method can be called</returns>
		Result CheckConfiguration();

		/// <summary>
		/// Gets the default connection string to use for this database
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		/// This should return a short descriptive title for the database
		/// provider, e.g. "SQL Server 2005" or "SQLite 3"
		/// </summary>
		string Title { get; }

		/// <summary>
		/// This event should be called from inside the <c cref="Initialise" />
		/// method. It is used to allow each module to perform initialisation
		/// tasks relating to this database handler.
		/// </summary>
		event InterruptableEventHandler OnInitialise;

		/// <summary>
		/// This should implement some form of generator that returns a unique long
		/// value for use as a database table row identifier.
		/// </summary>
		/// <returns>A unique long value</returns>
		long GetUniqueID();

		/// <summary>
		/// This method needs to track every call made to it. A first call to this method
		/// should persist the connection until an accompanying call to <c cref="ReleaseConnection" />
		/// is made. A stack should be used to keep track of how many sources are using this
		/// connection. Calls to <c cref="ReleaseConnection" /> should remove an item from the
		/// stack and if the stack is then empty, close the connection and nullify the reference.
		/// </summary>
		/// <returns>An IDBConnection which is open and ready for use</returns>
		IDbConnection GetConnection();

		/// <summary>
		/// This method should create a new connection for independent use. Responsibility for
		/// closing the connection and cleaning up resources is left to the caller.
		/// </summary>
		/// <returns>An IDBConnection which is open and ready for use</returns>
		IDbConnection CreateConnection();

		/// <summary>
		/// This method should create a new connection for independent use. Responsibility for
		/// closing the connection and cleaning up resources is left to the caller.
		/// </summary>
		/// <param name="connectionString">The connection string to use to open the connection</param>
		/// <returns>An IDBConnection which is open and ready for use</returns>
		IDbConnection CreateConnection(string connectionString);

		/// <summary>
		/// This should first remove an item from the stack. If the stack is then empty, the
		/// connection should be closed and disposed of.
		/// </summary>
		void ReleaseConnection();

		/// <summary>
		/// This will be called at the end of every request to ensure that database connections are always
		/// closed despite any errors that may have occurred.
		/// </summary>
		void ForceCloseConnection();
	}
}
