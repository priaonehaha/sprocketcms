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
					cmd.Parameters.Add(NewParameter("@PublishDate", page.PublishDate, DbType.DateTime));
					cmd.Parameters.Add(NewParameter("@ExpiryDate", page.ExpiryDate, DbType.DateTime));
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
		public Page SelectPageBySprocketPath(string sprocketPath)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["Select Page By RequestPath"], conn);
					cmd.Parameters.Add(new SQLiteParameter("@RequestPath", sprocketPath));
					cmd.Parameters.Add(new SQLiteParameter("@ExcludeDraft", true));
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
		public Result DeleteDraftRevisions(long pageID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = Procedures["Delete Draft Revisions"];
					cmd.Connection = connection;
					cmd.Parameters.Add(NewParameter("@RevisionSourceID", pageID, DbType.Int64));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("SQLiteContentDataProvider.DeleteDraftRevisions: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public PageResultSet ListPages(PageSearchOptions options)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(ReplacePageSortOrder(Procedures["List Pages"], options.PageOrder), conn);
					cmd.Parameters.Add(NewParameter("@Hidden", options.Hidden, DbType.Boolean));
					cmd.Parameters.Add(NewParameter("@Deleted", options.Deleted, DbType.Boolean));
					cmd.Parameters.Add(NewParameter("@Draft", options.Draft, DbType.Boolean));
					cmd.Parameters.Add(NewParameter("@Offset", Math.Max((options.PageNumber - 1) * options.PageSize, 0), DbType.Int64));
					cmd.Parameters.Add(NewParameter("@PageSize", options.PageSize <= 0 ? -1 : options.PageSize, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@CategorySetName", options.CategorySetName, DbType.String));
					cmd.Parameters.Add(NewParameter("@CategoryName", options.CategoryName, DbType.String));
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						reader.Read();
						long totalResults = Convert.ToInt64(reader[0]);
						reader.NextResult();
						List<Page> list = new List<Page>();
						while (reader.Read())
							list.Add(new Page(reader));
						reader.Close();
						return new PageResultSet(list, totalResults, options.PageSize, options.PageNumber, options.PageOrder);
					}
				}
			}
		}
		private string ReplacePageSortOrder(string sql, PageResultSetOrder pageOrder)
		{
			string dir, field;
			switch (pageOrder)
			{
				case PageResultSetOrder.PublishDateAscending:
					field = "r.RevisionDate";
					dir = "ASC";
					break;
				case PageResultSetOrder.PublishDateDescending:
					field = "r.RevisionDate";
					dir = "DESC";
					break;

				case PageResultSetOrder.Random:
					field = "random(*)";
					dir = "ASC";
					break;

				default:
					throw new Exception("Unexpected value for PageResultSetOrder (" + pageOrder + ")");
			}
			return sql.Replace("$FIELD", field).Replace("$DIRECTION", dir);
		}

		public Dictionary<string, List<EditFieldInfo>> ListPageEditFieldsByFieldType(long pageRevisionID)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["List EditFields For Page Revision"], conn);
					cmd.Parameters.Add(NewParameter("@PageRevisionID", pageRevisionID, DbType.Int64));
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						// group all of the nodes according to type so that they can have their individual type's id list loaded
						Dictionary<string, List<EditFieldInfo>> map = new Dictionary<string, List<EditFieldInfo>>();
						while (reader.Read())
						{
							EditFieldInfo field = new EditFieldInfo();
							if (!field.Read(reader))
								continue;

							List<EditFieldInfo> list;
							if (!map.TryGetValue(field.Handler.TypeName, out list))
							{
								list = new List<EditFieldInfo>();
								map.Add(field.Handler.TypeName, list);
							}
							list.Add(field);
						}
						reader.Close();
						return map;
					}
				}
			}
		}
		public Dictionary<string, List<EditFieldInfo>> ListPageEditFieldsByFieldType(List<long> pageRevisionIDs)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					StringBuilder ids = new StringBuilder();
					foreach (long id in pageRevisionIDs)
					{
						ids.Append(",");
						ids.Append(id);
					}
					SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM EditFieldInfo WHERE PageRevisionID IN (0" + ids + ") ORDER BY EditFieldTypeIdentifier", conn);
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						// group all of the nodes according to type so that they can have their individual type's id list loaded
						Dictionary<string, List<EditFieldInfo>> map = new Dictionary<string, List<EditFieldInfo>>();
						while (reader.Read())
						{
							EditFieldInfo field = new EditFieldInfo();
							if (!field.Read(reader))
								continue;

							List<EditFieldInfo> list;
							if (!map.TryGetValue(field.Handler.TypeName, out list))
							{
								list = new List<EditFieldInfo>();
								map.Add(field.Handler.TypeName, list);
							}
							list.Add(field);
						}
						reader.Close();
						return map;
					}
				}
			}
		}
		public Result StoreEditFieldInfo(long pageRevisionID, EditFieldInfo info)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					if (info.Data != null && info.DataID == 0)
						info.DataID = DatabaseManager.GetUniqueID();

					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = Procedures["Store EditFieldInfo"];
					cmd.Connection = connection;
					cmd.Parameters.Add(NewParameter("@PageRevisionID", pageRevisionID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@EditFieldID", info.DataID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@EditFieldTypeIdentifier", info.Handler.TypeName, DbType.String));
					cmd.Parameters.Add(NewParameter("@SectionName", info.SectionName, DbType.String));
					cmd.Parameters.Add(NewParameter("@FieldName", info.FieldName, DbType.String));
					cmd.Parameters.Add(NewParameter("@Rank", info.Rank, DbType.Int64));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("SQLiteContentDataProvider.StoreEditFieldInfo: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public Dictionary<string, List<string>> ListPageCategories(long pageRevisionID)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand(Procedures["List Categories for Page Revision"], conn);
					cmd.Parameters.Add(NewParameter("@PageRevisionID", pageRevisionID, DbType.Int64));
					Dictionary<string, List<string>> cats = new Dictionary<string, List<string>>();
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						while (reader.Read())
						{
							string catsetname = reader["CategorySetName"].ToString();
							string catname = reader["CategoryName"].ToString();
							List<string> catlist;
							if (!cats.TryGetValue(catsetname, out catlist))
							{
								catlist = new List<string>();
								cats.Add(catsetname, catlist);
							}
							catlist.Add(catname);
						}
						reader.Close();
						return cats;
					}
				}
			}
		}
		public Result StorePageCategories(long pageRevisionID, Dictionary<string, List<string>> categories)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = Procedures["Store Page Category"];
					cmd.Parameters.Add(new SQLiteParameter("@PageRevisionID", pageRevisionID));
					SQLiteParameter prmCatSetName = new SQLiteParameter("@CategorySetName", DbType.String);
					SQLiteParameter prmCatName = new SQLiteParameter("@CategoryName", DbType.String);
					cmd.Parameters.Add(prmCatSetName);
					cmd.Parameters.Add(prmCatName);
					foreach (KeyValuePair<string, List<string>> kvp in categories)
					{
						prmCatSetName.Value = kvp.Key;
						foreach (string cat in kvp.Value)
						{
							prmCatName.Value = cat;
							cmd.ExecuteNonQuery();
						}
					}
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("SQLiteContentDataProvider.StorePageCategories: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public Result StoreEditField_TextBox(long dataID, string text)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = Procedures["Store EditField_TextBox"];
					cmd.Connection = connection;
					cmd.Parameters.Add(NewParameter("@EditFieldID", dataID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@Value", text, DbType.String));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("SQLiteContentDataProvider.StoreEditField_TextBox: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}
		public void LoadDataList_TextBox(List<EditFieldInfo> fields)
		{
			if (fields.Count == 0)
				return;

			StringBuilder ids = new StringBuilder();
			Dictionary<long, EditFieldInfo> map = new Dictionary<long, EditFieldInfo>();
			foreach (EditFieldInfo info in fields)
				if (info.DataID > 0)
				{
					map.Add(info.DataID, info);
					if (ids.Length > 0)
						ids.Append(",");
					ids.Append(info.DataID);
				}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM EditField_TextBox WHERE EditFieldID IN (" + ids + ")", conn);
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						while (reader.Read())
						{
							TextBoxEditField.TextBoxData data = new TextBoxEditField.TextBoxData();
							data.Text = reader["Value"].ToString();
							map[Convert.ToInt64(reader["EditFieldID"])].Data = data;
						}
						reader.Close();
					}
				}
			}
		}

		public Result StoreEditField_Image(long dataID, long sprocketFileID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = Procedures["Store EditField_Image"];
					cmd.Connection = connection;
					cmd.Parameters.Add(NewParameter("@EditFieldID", dataID, DbType.Int64));
					cmd.Parameters.Add(NewParameter("@SprocketFileID", sprocketFileID, DbType.Int64));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("SQLiteContentDataProvider.StoreEditField_Image: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}
		public void LoadDataList_Image(List<EditFieldInfo> fields)
		{
			if (fields.Count == 0)
				return;

			StringBuilder ids = new StringBuilder();
			Dictionary<long, EditFieldInfo> map = new Dictionary<long, EditFieldInfo>();
			foreach (EditFieldInfo info in fields)
				if (info.DataID > 0)
				{
					map.Add(info.DataID, info);
					if (ids.Length > 0)
						ids.Append(",");
					ids.Append(info.DataID);
				}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				using (SQLiteConnection conn = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					conn.Open();
					SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM EditField_Image WHERE EditFieldID IN (" + ids + ")", conn);
					using (SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						while (reader.Read())
						{
							ImageEditField.ImageEditFieldData data = new ImageEditField.ImageEditFieldData();
							data.SprocketFileID = Convert.ToInt64(reader["SprocketFileID"]);
							map[Convert.ToInt64(reader["EditFieldID"])].Data = data;
						}
						reader.Close();
					}
				}
			}
		}
	}
}
