using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Transactions;

using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Web.FileManager.SQLite
{
	public class SQLiteFileManagerDataLayer : IFileManagerDataLayer
	{
		SQLiteStoredProcedures procs;
		public SQLiteFileManagerDataLayer()
		{
			procs = new SQLiteStoredProcedures(ResourceLoader.LoadTextResource("Sprocket.Web.FileManager.SQLite.procedures.sql"));
		}

		public Type DatabaseHandlerType
		{
			get { return typeof(SQLiteDatabase); }
		}

		public void InitialiseDatabase(Result result)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = ResourceLoader.LoadTextResource("Sprocket.Web.FileManager.SQLite.schema.sql");
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				result.SetFailed(ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public SQLiteParameter NewSqlParameter(string name, object value, DbType dbType)
		{
			SQLiteParameter prm = new SQLiteParameter(name, value);
			prm.DbType = dbType;
			return prm;
		}

		public event InterruptableEventHandler<SprocketFile> OnBeforeDeleteSprocketFile;
		public event NotificationEventHandler<SprocketFile> OnSprocketFileDeleted;

		public Result Store(SprocketFile sprocketFile)
		{
			SQLiteConnection conn = null;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					conn = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = new SQLiteCommand();
					cmd.Connection = conn;
					if (sprocketFile.SprocketFileID == 0)
					{
						sprocketFile.SprocketFileID = DatabaseManager.GetUniqueID();
						cmd.CommandText = procs["Insert SprocketFile"];
					}
					else if (SelectSprocketFile(sprocketFile.SprocketFileID, false) == null)
						cmd.CommandText = procs["Insert SprocketFile"];
					else
						cmd.CommandText = procs["Update SprocketFile"];
					cmd.Parameters.Add(NewSqlParameter("@SprocketFileID", sprocketFile.SprocketFileID, DbType.Int64));
					cmd.Parameters.Add(NewSqlParameter("@ClientSpaceID", sprocketFile.ClientSpaceID, DbType.Int64));
					cmd.Parameters.Add(NewSqlParameter("@FileData", sprocketFile.FileData, DbType.Object));
					cmd.Parameters.Add(NewSqlParameter("@FileTypeExtension", sprocketFile.FileTypeExtension, DbType.String));
					cmd.Parameters.Add(NewSqlParameter("@OriginalFileName", sprocketFile.OriginalFileName, DbType.String));
					cmd.Parameters.Add(NewSqlParameter("@ContentType", sprocketFile.ContentType, DbType.String));
					cmd.Parameters.Add(NewSqlParameter("@Title", sprocketFile.Title, DbType.String));
					cmd.Parameters.Add(NewSqlParameter("@Description", sprocketFile.Description, DbType.String));
					cmd.Parameters.Add(NewSqlParameter("@UploadDate", sprocketFile.UploadDate, DbType.DateTime));
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

		public Result Delete(SprocketFile sprocketFile)
		{
			Result result = new Result();
			if (OnBeforeDeleteSprocketFile != null)
				OnBeforeDeleteSprocketFile(sprocketFile, result);
			SQLiteConnection conn = null;
			if (result.Succeeded)
			{
				try
				{
					using (TransactionScope scope = new TransactionScope())
					{
						conn = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
						SQLiteCommand cmd = new SQLiteCommand(procs["Delete SprocketFile"], conn);
						cmd.Parameters.Add(new SQLiteParameter("@SprocketFileID", sprocketFile.SprocketFileID));
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
				if (OnSprocketFileDeleted != null)
					OnSprocketFileDeleted(sprocketFile);
			}
			return result;
		}

		public SprocketFile SelectSprocketFile(long id, bool getFileData)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["Select SprocketFile"];
					cmd.Parameters.Add(new SQLiteParameter("@SprocketFileID", id));
					cmd.Parameters.Add(new SQLiteParameter("@GetFileData", getFileData));
					SQLiteDataReader reader = cmd.ExecuteReader();
					SprocketFile entity;
					if (!reader.Read())
						entity = null;
					else
						entity = new SprocketFile(reader);
					reader.Close();
					scope.Complete();
					return entity;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}
	}
}
