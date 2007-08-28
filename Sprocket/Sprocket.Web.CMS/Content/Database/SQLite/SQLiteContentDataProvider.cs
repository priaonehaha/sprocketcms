using System;
using System.Data;
using System.Data.SQLite;
using System.Transactions;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content.Database.SQLite
{
	class SQLiteContentDataProvider //: ISQLiteContentDataProvider
	{
		private SQLiteStoredProcedures procs;
		internal SQLiteStoredProcedures Procedures
		{
			get { return procs; }
		}

		public SQLiteContentDataProvider()
		{
			procs = new SQLiteStoredProcedures(ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Database.SQLite.procedures.sql"));
		}

		public Result Initialise()
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Database.SQLite.schema.sql");
					cmd.ExecuteNonQuery();
					cmd.Connection = connection;
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result(ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		internal SQLiteParameter NewParameter(string name, object value, DbType type)
		{
			SQLiteParameter prm = new SQLiteParameter(name);
			if (value == null)
				prm.Value = DBNull.Value;
			else
				prm.Value = value;
			prm.DbType = type;
			return prm;
		}

		public Result Store(RevisionInformation revisionInformation)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = Procedures["Insert Revision"];
					cmd.Parameters.Add(NewParameter("@RevisionID", revisionInformation.RevisionID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@RevisionGroupID", revisionInformation.RevisionGroupID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@RevisionDate", revisionInformation.RevisionDate, DbType.DateTime));
					cmd.Parameters.Add(NewParameter("@UserID", revisionInformation.UserID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@Notes", revisionInformation.Notes, DbType.String));
					cmd.Parameters.Add(NewParameter("@Deleted", revisionInformation.Deleted, DbType.Boolean));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result(ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}
	}
}
