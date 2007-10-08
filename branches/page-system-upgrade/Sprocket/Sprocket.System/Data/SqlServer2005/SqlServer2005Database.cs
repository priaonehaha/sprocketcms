using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using System.Text.RegularExpressions;

using Sprocket.Web;
using Sprocket.Utility;

namespace Sprocket.Data
{
	public class SqlServer2005Database : IDatabaseHandler
	{
		private string connectionString = null;

		public Result Initialise()
		{
			Result result;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SqlConnection conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					result = ExecuteScript(conn, ResourceLoader.LoadTextResource("Sprocket.Data.SqlServer2005.scripts.sql"));
					if (result.Succeeded && OnInitialise != null)
						OnInitialise(result);
					if (result.Succeeded)
						scope.Complete();
					return result;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

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
			catch (Exception ex)
			{
				return new Result("The ConnectionString value was unable to be used to open the database. The error was: " + ex.Message);
			}
			return new Result();
		}

		public string Title
		{
			get { return "SQL Server 2005"; }
		}

		public event InterruptableEventHandler OnInitialise;

		public long GetUniqueID()
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SqlConnection conn = new SqlConnection(connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand("GetUniqueID", conn);
					SqlParameter prm = new SqlParameter("@ID", SqlDbType.BigInt);
					prm.Direction = ParameterDirection.Output;
					cmd.Parameters.Add(prm);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.ExecuteNonQuery();
					conn.Close();
					return (long)prm.Value;
				}
			}
		}

		public Result ExecuteScript(SqlConnection conn, string script)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				string[] sql = Regex.Split(script, @"^[ \t]*go[ \t\r\n]*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				for (int i = 0; i < sql.Length; i++)
				{
					if (sql[i].Trim() == "")
						continue;
					using (TransactionScope innerscope = new TransactionScope())
					{
						SqlCommand cmd = conn.CreateCommand();
						cmd.CommandText = sql[i];
						cmd.CommandType = CommandType.Text;
						try
						{
							cmd.ExecuteNonQuery();
						}
						catch (Exception ex)
						{
							return new Result(ex.Message + Environment.NewLine + Environment.NewLine + "The offending SQL was:" + Environment.NewLine + sql[i]);
						}
						innerscope.Complete();
					}
				}
				scope.Complete();
				return new Result();
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
				SqlConnection conn = (SqlConnection)CreateConnection();
				Conn = conn;
				return conn;
			}
			else
			{
				SqlConnection c = Conn as SqlConnection;
				if (c != null)
				{
					if (c.State == ConnectionState.Open)
						return Conn as SqlConnection;
					c.Dispose();
					Conn = null;
				}
				stack = null;
				return GetConnection();
			}
		}

		public void ReleaseConnection()
		{
			if (stack == null)
				stack = new Stack<bool>();
			if (stack.Count == 1)
			{
				SqlConnection conn = Conn as SqlConnection;
				if (conn != null)
				{
					conn.Close();
					conn.Dispose();
					Conn = null;
				}
			}
			if(stack.Count > 0)
				stack.Pop();
		}

		public void ForceCloseConnection()
		{
			SqlConnection c = Conn as SqlConnection;
			if (c != null)
			{
				if (c.State == ConnectionState.Open)
					c.Close();
				c.Dispose();
				Conn = null;
				stack = null;
			}
		}

		private SqlConnection Conn
		{
			get { return CurrentRequest.Value["PersistedSqlConnection.SqlServer2005"] as SqlConnection; }
			set { CurrentRequest.Value["PersistedSqlConnection.SqlServer2005"] = value; }
		}

		public IDbConnection CreateConnection()
		{
			return CreateConnection(ConnectionString);
		}

		public IDbConnection CreateConnection(string connectionString)
		{
			SqlConnection conn = new SqlConnection(connectionString);
			conn.Open();
			return conn;
		}
	}
}
