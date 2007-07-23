using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;
using System.Transactions;

using Sprocket.Utility;
using Sprocket.Web;

namespace Sprocket.Data
{
	public class SQLiteDatabase : IDatabaseHandler
	{
		SQLiteStoredProcedures procs;
		public SQLiteDatabase()
		{
			procs = new SQLiteStoredProcedures(ResourceLoader.LoadTextResource("Sprocket.Data.SQLite.scripts.sql"));
		}

		private string connectionString;
		public string ConnectionString
		{
			get
			{
				if (connectionString == null)
					connectionString = CreateConnectionString("datastore/databases/main.s3db");
				return connectionString;
			}
		}

		public static string CreateConnectionString(string sprocketPath)
		{
			return "Data Source=" + WebUtility.MapPath(sprocketPath) + ";UseUTF16Encoding=True;";
		}

		public static void CheckFileExists(string connectionString)
		{
			string path = Regex.Match(connectionString, "Data Source=(?<path>[^;]+)").Groups["path"].Value;
			if (!File.Exists(path))
			{
				FileInfo info = new FileInfo(path);
				if (!info.Directory.Exists)
					info.Directory.Create();
				SQLiteConnection.CreateFile(path);
			}
		}

		public Result Initialise()
		{
			try
			{
				CheckFileExists(ConnectionString);
			}
			catch (Exception ex)
			{
				return new Result("Unable to initialise the default SQLite database: " + ex.Message);
			}

			Result result = new Result();
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["Schema"];
					cmd.CommandType = CommandType.Text;
					try
					{
						cmd.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						return new Result(ex.Message);
					}

					if (OnInitialise != null)
						OnInitialise(result);

					if(result.Succeeded)
						scope.Complete();
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}

			return result;
		}

		public Result CheckConfiguration()
		{
			return new Result();
		}

		public string Title
		{
			get { return "SQLite 3"; }
		}

		public event InterruptableEventHandler OnInitialise;

		public long GetUniqueID()
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["GetUniqueID"];
					cmd.CommandType = CommandType.Text;
					SQLiteDataReader reader = cmd.ExecuteReader();
					reader.Read();
					long id = (long)reader[0];
					reader.Close();
					scope.Complete();
					return id;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		private Stack<bool> stack = new Stack<bool>();
		public IDbConnection GetConnection()
		{
			if (stack == null)
				stack = new Stack<bool>();
			stack.Push(true);
			if (stack.Count == 1)
			{
				SQLiteConnection conn = (SQLiteConnection)CreateConnection();
				Conn = conn;
				return conn;
			}
			else
			{
				SQLiteConnection c = Conn as SQLiteConnection;
				if (c != null)
				{
					if (c.State == ConnectionState.Open)
						return Conn as SQLiteConnection;
					c.Dispose();
					Conn = null;
				}
				stack = null;
				return GetConnection();
			}
		}

		public IDbConnection CreateConnection()
		{
			return CreateConnection(ConnectionString);
		}

		public IDbConnection CreateConnection(string connectionString)
		{
			CheckFileExists(connectionString);
			SQLiteConnection conn = new SQLiteConnection(connectionString);
			conn.Open();
			return conn;
		}

		public void ReleaseConnection()
		{
			if (stack.Count == 1)
			{
				SQLiteConnection conn = Conn as SQLiteConnection;
				if (conn != null)
				{
					if (conn.State == ConnectionState.Open)
						conn.Close();
					conn.Dispose();
					Conn = null;
				}
			}
			if (stack.Count > 0)
				stack.Pop();
		}

		private SQLiteConnection Conn
		{
			get { return CurrentRequest.Value["PersistedSqlConnection.Sqlite3"] as SQLiteConnection; }
			set { CurrentRequest.Value["PersistedSqlConnection.Sqlite3"] = value; }
		}

		public void ForceCloseConnection()
		{
			SQLiteConnection c = Conn as SQLiteConnection;
			if (c != null)
			{
				if (c.State == ConnectionState.Open)
					c.Close();
				c.Dispose();
				Conn = null;
				stack = null;
			}
		}
	}
}
