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
	class SQLiteContentDataProvider : SQLiteDataProvider, IContentDataProvider
	{
		protected override string SchemaResourceNamespace { get { return "Sprocket.Web.CMS.Database.SQLite.schema.sql"; } }
		protected override string ProceduresResourceNamespace { get { return "Sprocket.Web.CMS.Database.SQLite.procedures.sql"; } }

		public Result Store(RevisionInformation revisionInformation)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection conn = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["Store RevisionInformation"], conn);
					if (revisionInformation.RevisionID == 0)
						revisionInformation.RevisionID = DatabaseManager.GetUniqueID();
					cmd.Parameters.Add(new SQLiteParameter("@RevisionID", revisionInformation.RevisionID));
					cmd.Parameters.Add(NewParameter("@RevisionSourceID", revisionInformation.RevisionSourceID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@RevisionDate", revisionInformation.RevisionDate, DbType.DateTime));
					cmd.Parameters.Add(NewParameter("@UserID", revisionInformation.UserID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@Notes", revisionInformation.Notes, DbType.String));
					cmd.Parameters.Add(NewParameter("@Hidden", revisionInformation.Hidden, DbType.Boolean));
					cmd.Parameters.Add(NewParameter("@Draft", revisionInformation.Draft, DbType.Boolean));
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
		public Result Store(Page page)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection conn = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["Store Page"], conn);
					if (page.PageID == 0)
						page.PageID = DatabaseManager.GetUniqueID();
					cmd.Parameters.Add(new SQLiteParameter("@PageID", page.PageID));
					cmd.Parameters.Add(NewParameter("@RevisionID", page.RevisionID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@PageCode", page.PageCode, DbType.String));
					cmd.Parameters.Add(NewParameter("@ParentPageCode", page.ParentPageCode, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@TemplateName", page.TemplateName, DbType.String));
					cmd.Parameters.Add(NewParameter("@Requestable", page.Requestable, DbType.Boolean));
					cmd.Parameters.Add(NewParameter("@RequestPath", page.RequestPath, DbType.String));
					cmd.Parameters.Add(NewParameter("@ContentType", page.ContentType, DbType.String));
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

		public List<Page> ListPages()
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["List Pages"], conn);
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						List<Page> list = new List<Page>();
						while (reader.Read())
							list.Add(new Page(reader));
						reader.Close();
						return list;
					}
				}
			}
		}
	}
}
