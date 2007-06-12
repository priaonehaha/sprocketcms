using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;
using System.Transactions;

using Sprocket.Web;

namespace Sprocket.Data
{
	public class SQLiteDatabase : IDatabaseHandler
	{
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

			if (OnInitialise != null)
				OnInitialise(result);
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
			return -1;
		}

		private Stack<bool> stack = new Stack<bool>();
		public IDbConnection GetConnection()
		{
			stack.Push(true);
			if (stack.Count == 1)
			{
				SQLiteConnection conn = (SQLiteConnection)CreateConnection();
				Conn = conn;
				return conn;
			}
			else
			{
				return Conn as SQLiteConnection;
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
			stack.Pop();
			if (stack.Count == 0)
			{
				SQLiteConnection conn = Conn as SQLiteConnection;
				conn.Close();
				conn.Dispose();
				Conn = null;
			}
		}

		private SQLiteConnection Conn
		{
			get { return CurrentRequest.Value["PersistedSqlConnection.Sqlite3"] as SQLiteConnection; }
			set { CurrentRequest.Value["PersistedSqlConnection.Sqlite3"] = value; }
		}

		public void ForceCloseConnection()
		{
		}
	}
}
