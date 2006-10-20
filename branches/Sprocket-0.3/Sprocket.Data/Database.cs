using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket;
using Sprocket.Web;
using Sprocket.SystemBase;

namespace Sprocket.Data
{
	public enum DatabaseEngine
	{
		SqlServer
	}

	public abstract class Database
	{
		public static string DefaultName
		{
			get
			{
				string name;
				if (CurrentRequest.Value["SPROCKET_DEFAULTDBNAME"] == null)
					name = "";
				else
					name = CurrentRequest.Value["SPROCKET_DEFAULTDBNAME"].ToString();
				return name;
			}

			set
			{
				CurrentRequest.Value["SPROCKET_DEFAULTDBNAME"] = value;
			}
		}

		private static Dictionary<string, Database> databases = new Dictionary<string, Database>();
		public static Database Get(string name)
		{
			if (CurrentRequest.Value["SPROCKET_DATABASE_REG"] != null)
			{
				Dictionary<string, Database> dblist = (Dictionary<string, Database>)CurrentRequest.Value["SPROCKET_DATABASE_REG"];
				if (dblist[name] != null)
					return dblist[name];
			}
			throw new SprocketException("No database has been registered under the name " + name + ".");
		}

		public static Database Main
		{
			get
			{
				if (DefaultName == "")
					((DatabaseManager)SystemCore.Instance["DatabaseManager"]).LoadDefaultDatabase();
				return Get(DefaultName);
			}
		}

		public static void Add(string name, Database db, bool isMain)
		{
			Dictionary<string, Database> dblist;
			if(CurrentRequest.Value["SPROCKET_DATABASE_REG"] == null)
			{
				dblist = new Dictionary<string, Database>();
				CurrentRequest.Value["SPROCKET_DATABASE_REG"] = dblist;
			}
			else
				dblist = (Dictionary<string,Database>)CurrentRequest.Value["SPROCKET_DATABASE_REG"];
			dblist.Add(name, db);
			if (isMain)
				Database.DefaultName = name;
		}

		private DatabaseEngine databaseEngine;
		public static Database Create(DatabaseEngine engine)
		{
			Database db;
			switch (engine)
			{
				case DatabaseEngine.SqlServer:
					db = new SqlDatabase();
					break;
				default:
					throw new NotImplementedException(engine.ToString() + " not supported.");
			}
			db.databaseEngine = engine;
			return db;
		}

		public static void CloseAll()
		{
			if (CurrentRequest.Value["SPROCKET_DATABASE_REG"] != null)
			{
				foreach (KeyValuePair<string,Database> kvp in (Dictionary<string, Database>)CurrentRequest.Value["SPROCKET_DATABASE_REG"])
					if(kvp.Value.IsConnectionOpen)
						kvp.Value.CloseConnection();
			}
		}

		public DatabaseEngine DatabaseEngine
		{
			get { return databaseEngine; }
		}

		#region Static Helper Functions
		public static DatabaseEngine ParseEngineName(string name)
		{
			try { return (DatabaseEngine)Enum.Parse(typeof(DatabaseEngine), name, true); }
			catch { throw new SprocketException("Specified database type does not exist (" + name + ")"); }
		}

		public static object Coalesce(object val, object valIfNull)
		{
			return (val == null || val == DBNull.Value) ? valIfNull : val;
		}

		public static Guid GetGuid(object value)
		{
			if (value == null || value == DBNull.Value) return Guid.Empty;
			if (value is Guid) return (Guid)value;
			if (value is string)
			{
				try { return new Guid(value.ToString()); }
				catch { }
			}
			throw new SprocketException("Cannot convert value to Guid. Format is invalid: " + value);
		}
		#endregion

		#region Command Handling
		public IDbCommand CreateCommand(string commandText, CommandType commandType)
		{
			if (!IsConnectionOpen) OpenConnection();
			IDbCommand cmd = conn.CreateCommand();
			cmd.CommandText = commandText;
			cmd.CommandType = commandType;
			if (trans != null) cmd.Transaction = trans;
			return cmd;
		}

		public IDataParameter AddParameter(IDbCommand cmd, string name, object value)
		{
			IDataParameter prm = cmd.CreateParameter();
			prm.ParameterName = name;
			prm.Value = value == null ? DBNull.Value : value;
			CheckParameter(prm);
			cmd.Parameters.Add(prm);
			return prm;
		}

		public IDataParameter AddParameter(IDbCommand cmd, string name, object value, DbType dbtype)
		{
			IDataParameter prm = cmd.CreateParameter();
			prm.ParameterName = name;
			prm.Value = value == null ? DBNull.Value : value;
			prm.DbType = dbtype;
			CheckParameter(prm);
			cmd.Parameters.Add(prm);
			return prm;
		}

		public IDataParameter AddOutputParameter(IDbCommand cmd, string name, DbType dbType)
		{
			IDataParameter prm = cmd.CreateParameter();
			prm.Direction = ParameterDirection.Output;
			prm.ParameterName = name;
			prm.Value = DBNull.Value;
			prm.DbType = dbType;
			CheckParameter(prm);
			cmd.Parameters.Add(prm);
			return prm;
		}

		public IDataParameter AddInputOutputParameter(IDbCommand cmd, string name, DbType dbType, object value)
		{
			IDataParameter prm = cmd.CreateParameter();
			prm.Direction = ParameterDirection.InputOutput;
			prm.ParameterName = name;
			prm.Value = value == null ? DBNull.Value : value;
			prm.DbType = dbType;
			CheckParameter(prm);
			cmd.Parameters.Add(prm);
			return prm;
		}

		protected virtual void CheckParameter(IDataParameter prm)
		{
		}

		public virtual void SetParameterSize(IDataParameter prm, int size)
		{
		}

		public DataSet GetDataSet(IDbCommand cmd)
		{
			IDataAdapter da = GetDataAdapter(cmd);
			DataSet ds = new DataSet();
			da.Fill(ds);
			return ds;
		}

		public abstract IDataAdapter GetDataAdapter(IDbCommand cmd);
		public abstract IDataAdapter GetDataAdapter();
		public abstract void SetDataAdapterCommands(IDataAdapter da, IDbCommand select, IDbCommand update, IDbCommand insert, IDbCommand delete);
		#endregion

		#region Connection String
		protected string connString = "";

		public string ConnectionString
		{
			get
			{
				return connString;
			}
			set
			{
				AssertConnectionClosed();
				connString = value;
			}
		}

		public void LoadConnectionString(string appSettingsKeyName)
		{
			if (SprocketSettings.GetValue(appSettingsKeyName) == null)
				throw new SprocketException("The application settings file does not contain a connection string for the key \"" + appSettingsKeyName + "\".");
			ConnectionString = SprocketSettings.GetValue(appSettingsKeyName);
		}

		public virtual bool TestConnectionString(out string errorMessage)
		{
			try { OpenConnection(); }
			catch(SprocketException ex)
			{
				errorMessage = ex.Message;
				return false;
			}
			errorMessage = "";
			CloseConnection();
			return true;
		}
		#endregion

		#region Connection Handling
		protected IDbConnection conn = null;
		
		public bool IsConnectionOpen
		{
			get { return conn != null; }
		}

		public abstract IDbConnection GetConnectionObject();

		public IDbConnection OpenConnection()
		{
			AssertConnectionStringExists();
			AssertConnectionClosed();
			conn = GetConnectionObject();
			try { conn.ConnectionString = ConnectionString; }
			catch { throw new SprocketException("The supplied connection string is not in a valid format."); }
			try { conn.Open(); }
			catch { throw new SprocketException("Cannot open connection using connection string: " + ConnectionString); }
			return conn;
		}

		public void CloseConnection()
		{
			AssertConnectionExists();
			AssertTransactionInActive();
			conn.Close();
			conn = null;
		}

		protected bool previousOpenState = false;
		public void RememberOpenState()
		{
			previousOpenState = conn == null;
		}
		public void CloseIfWasntOpen()
		{
			if (previousOpenState && conn != null)
				CloseConnection();
			previousOpenState = false;
		}
		#endregion

		#region Transaction Handling
		protected IDbTransaction trans = null;

		public bool IsTransactionActive
		{
			get { return trans != null; }
		}

		public IDbTransaction BeginTransaction()
		{
			AssertTransactionInActive();

			if (!IsConnectionOpen) OpenConnection();
			trans = conn.BeginTransaction();
			return trans;
		}

		public void CommitTransaction()
		{
			AssertConnectionExists();
			AssertTransactionActive();

			trans.Commit();
			trans = null;
			conn.Close();
			conn = null;
		}

		public void RollbackTransaction()
		{
			AssertConnectionExists();
			AssertTransactionActive();

			trans.Rollback();
			trans = null;
			conn.Close();
			conn = null;
		}
		#endregion

		#region Assert Definitions
		protected void AssertConnectionStringExists()
		{
			if (connString == null || connString == "")
				throw new SprocketException("Connection string has not been set.");
		}

		protected void AssertConnectionExists()
		{
			if (conn == null)
				throw new SprocketException("Database connection is not currently open.");
		}

		protected void AssertConnectionClosed()
		{
			if (conn != null)
				throw new SprocketException("Database connection is already open.");
		}

		protected void AssertTransactionInActive()
		{
			if (trans != null)
				throw new SprocketException("A transaction is already in progress.");
		}

		protected void AssertTransactionActive()
		{
			if (trans == null)
				throw new SprocketException("No transaction currently exists for this connection.");
		}
		#endregion
	}
}
