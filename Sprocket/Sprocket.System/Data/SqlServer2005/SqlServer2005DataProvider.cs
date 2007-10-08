using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using System.Reflection;

using Sprocket.Utility;

namespace Sprocket.Data
{
	public abstract class SqlServer2005DataProvider : IModuleDataProvider
	{
		private Assembly asm;

		public SqlServer2005DataProvider()
		{
			asm = Assembly.GetCallingAssembly();
		}

		public Type DatabaseHandlerType
		{
			get { return typeof(SqlServer2005Database); }
		}

		public void Initialise(Result result)
		{
			string schemaNS = SchemaResourceNamespace;
			string procNS = ProceduresResourceNamespace;
			if (schemaNS == null || schemaNS == String.Empty || procNS == null || procNS == String.Empty)
				return;

			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SqlConnection conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					result.Merge(((SqlServer2005Database)DatabaseManager.DatabaseEngine).ExecuteScript(conn, ResourceLoader.LoadTextResource(asm, schemaNS)));
					result.Merge(((SqlServer2005Database)DatabaseManager.DatabaseEngine).ExecuteScript(conn, ResourceLoader.LoadTextResource(asm, procNS)));
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				result.SetFailed(GetType().FullName + ".Initialise: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		protected SqlParameter NewParameter(string name, object value, DbType type)
		{
			SqlParameter prm = new SqlParameter(name, null);
			prm.DbType = type;
			if (value == null)
				prm.Value = DBNull.Value;
			else
				prm.Value = value;
			return prm;
		}

		protected abstract string SchemaResourceNamespace { get; }
		protected abstract string ProceduresResourceNamespace { get; }
	}
}
