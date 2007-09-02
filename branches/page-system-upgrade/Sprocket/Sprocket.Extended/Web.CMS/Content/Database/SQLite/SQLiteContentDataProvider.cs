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
		protected override string SchemaResourceNamespace { get { return "Sprocket.Web.CMS.Content.Database.SQLite.schema.sql"; } }
		protected override string ProceduresResourceNamespace { get { return "Sprocket.Web.CMS.Content.Database.SQLite.procedures.sql"; } }

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
					cmd.Parameters.Add(NewParameter("@PageName", page.PageName, DbType.String));
					cmd.Parameters.Add(NewParameter("@PageCode", page.PageCode, DbType.String));
					cmd.Parameters.Add(NewParameter("@ParentPageCode", page.ParentPageCode, DbType.String));
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

		public Page SelectPage(long pageID)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["Select Page By PageID"], conn);
					cmd.Parameters.Add(new SQLiteParameter("@PageID", pageID));
					cmd.Parameters.Add(new SQLiteParameter("@ExcludeDraft", false));
					SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					Page entity;
					if (!reader.Read())
						entity = null;
					else
						entity = new Page(reader);
					reader.Close();
					return entity;
				}
			}
		}
		public Page SelectPageByPageCode(string pageCode)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["Select Page"], conn);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SQLiteParameter("@PageCode", pageCode));
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						Page entity;
						if (!reader.Read())
							entity = null;
						else
							entity = new Page(reader);
						reader.Close();
						return entity;
					}
				}
			}
		}
		public RevisionInformation SelectRevisionInformation(long revisionID)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["Select RevisionInformation"], conn);
					cmd.Parameters.Add(new SQLiteParameter("@RevisionID", revisionID));
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						RevisionInformation entity;
						if (!reader.Read())
							entity = null;
						else
							entity = new RevisionInformation(reader);
						reader.Close();
						return entity;
					}
				}
			}
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
