using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;
using System.Transactions;
using System.Reflection;
using Sprocket.Utility;
using Sprocket.Web;

namespace Sprocket.Data
{
	public abstract class SQLiteDataProvider : IModuleDataProvider
	{
		private Assembly asm;
		private SQLiteStoredProcedures procs;

		protected SQLiteStoredProcedures Procedures
		{
			get { return procs; }
		}

		public SQLiteDataProvider()
		{
			asm = Assembly.GetCallingAssembly();
			string ns = ProceduresResourceNamespace;
			if (ns == null || ns == String.Empty)
				procs = new SQLiteStoredProcedures(String.Empty);
			else
				procs = new SQLiteStoredProcedures(ResourceLoader.LoadTextResource(asm, ns));
		}

		public Type DatabaseHandlerType
		{
			get { return typeof(SQLiteDatabase); }
		}

		public virtual void Initialise(Result result)
		{
			string ns = SchemaResourceNamespace;
			if (ns == null || ns == String.Empty)
				return;

			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = ResourceLoader.LoadTextResource(asm, ns);
					cmd.ExecuteNonQuery();
					cmd.Connection = connection;
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				result.SetFailed(SchemaResourceNamespace + ": " + Environment.NewLine + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		protected SQLiteParameter NewParameter(string name, object value, DbType type)
		{
			SQLiteParameter prm = new SQLiteParameter(name);
			if (value == null)
				prm.Value = DBNull.Value;
			else
				prm.Value = value;
			prm.DbType = type;
			return prm;
		}

		protected abstract string SchemaResourceNamespace { get; }
		protected abstract string ProceduresResourceNamespace { get; }
	}
}
