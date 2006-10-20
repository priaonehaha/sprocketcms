using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

namespace Sprocket.Data
{
	public class SqlServer2005Database : IDatabaseHandler
	{
		private const string dbSprocketPath = "datastore/databases/main.sdb";
		public DbConnection CreateDefaultConnection()
		{
			return new SqlConnection(connectionString);
		}

		public Result Initialise()
		{
			return new Result();
		}

		private string connectionString = null;
		public string ConnectionString
		{
		  get { return connectionString; }
		}

		public Result CheckConfiguration()
		{
			connectionString = SprocketSettings.GetValue("ConnectionString");
			if (connectionString == null)
				return new Result("No value exists in Web.config for ConnectionString. SqlServer2005Database requires a valid connection string.");
			try
			{
				SqlConnection conn = new SqlConnection(connectionString);
				conn.Open();
				conn.Close();
				conn.Dispose();
			}
			catch(Exception ex)
			{
				return new Result("The ConnectionString value was unable to be used to open the database. The error was: " + ex.Message);
			}
			return new Result();
		}
	}
}
