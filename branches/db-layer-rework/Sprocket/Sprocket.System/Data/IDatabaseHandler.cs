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
	}
}
