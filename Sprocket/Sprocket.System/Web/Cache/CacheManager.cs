using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;
using System.Data.SQLite;
using System.Transactions;
using System.Threading;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Utility;

namespace Sprocket.Web.Cache
{
	internal class CacheManager
	{
		private class LockObjectCount
		{
			public object Object = new object();
			public int Count = 1;
		}

		Dictionary<string, LockObjectCount> lockObjects = new Dictionary<string, LockObjectCount>();
		private object GetLockObject(string identifier)
		{
			lock (lockObjects)
			{
				LockObjectCount c;
				if (lockObjects.TryGetValue(identifier, out c))
				{
					c.Count++;
				}
				else
				{
					c = new LockObjectCount();
					lockObjects.Add(identifier, c);
				}
				return c;
			}
		}
		private void ReleaseLockObject(string identifier)
		{
			lock (lockObjects)
			{
				LockObjectCount c;
				if (lockObjects.TryGetValue(identifier, out c))
				{
					c.Count--;
					if (c.Count == 0)
						lockObjects.Remove(identifier);
				}
			}
		}

		private string basePath;
		private SQLiteConnection conn = null;
		private string sqlInsert, sqlDelete, sqlDeletePartialMatches, sqlUpdate, sqlSelect, sqlSelectAll;
		private Dictionary<string, CacheItemInfo> sprocketPathMemoryCache = new Dictionary<string, CacheItemInfo>();
		private Dictionary<string, CacheItemInfo> memoryCache = new Dictionary<string, CacheItemInfo>();
		private long diskSpace = 0;

		public long DiskSpaceUsed
		{
			get { return diskSpace; }
		}

		public void Initialise()
		{
			basePath = WebUtility.MapPath("datastore/content-cache") + @"\";
			conn = new SQLiteConnection(SQLiteDatabase.CreateConnectionString("datastore/content-cache/cache.s3db"));
			SQLiteDatabase.CheckFileExists(conn.ConnectionString);
			conn.Open();
			SQLiteCommand cmd = new SQLiteCommand(ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-init.sql"), conn);
			cmd.ExecuteNonQuery();
			cmd = new SQLiteCommand(ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-select-all.sql"), conn);
			SQLiteDataReader reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				CacheItemInfo info = new CacheItemInfo(reader);
				if (File.Exists(info.PhysicalPath))
				{
					if (info.SprocketPath != null)
						sprocketPathMemoryCache.Add(info.SprocketPath, info);
					memoryCache.Add(info.IdentifierString, info);
					diskSpace += info.File.Length;
				}
			}
			reader.Close();
			sqlInsert = ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-insert.sql");
			sqlDelete = ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-delete.sql");
			sqlDeletePartialMatches = ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-delete-partial-matches.sql");
			sqlUpdate = ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-update.sql");
			sqlSelect = ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-select.sql");
			sqlSelectAll = ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-select-from-path.sql");
		}

		~CacheManager()
		{
			if (conn != null)
				if (conn.State == ConnectionState.Open)
					conn.Close();
		}

		public void FlushCache(long toSize)
		{
			lock (lockObjects)
			{
				SQLiteCommand cmd = new SQLiteCommand(ResourceLoader.LoadTextResource("Sprocket.Web.Cache.cache-select-all.sql"), conn);
				SQLiteDataReader reader = cmd.ExecuteReader();
				long size = 0;
				bool killitems = false;
				lock (memoryCache)
					while (reader.Read())
					{
						CacheItemInfo info = new CacheItemInfo(reader);
						if (killitems)
						{
							memoryCache.Remove(info.IdentifierString);
							DeleteFile(info.File);
						}
						else
						{
							size += info.File.Length;
							if (size >= toSize)
								killitems = true;
						}

					}
				diskSpace = size;
				reader.Close();
			}
		}

		public void Clear(string identifier)
		{
			lock (GetLockObject(identifier))
			{
				try
				{
					lock (memoryCache)
						memoryCache.Remove(identifier);

					SQLiteCommand cmd = new SQLiteCommand(sqlSelect, conn);
					cmd.Parameters.Add(new SQLiteParameter("@Identifier", identifier));
					SQLiteDataReader reader = cmd.ExecuteReader();
					if (reader.Read())
					{
						CacheItemInfo info = new CacheItemInfo(reader);
						DeleteFile(info.File);
						cmd = new SQLiteCommand(sqlDelete, conn);
						cmd.Parameters.Add(new SQLiteParameter("@Identifier", info.IdentifierString));
						cmd.ExecuteNonQuery();
					}
					reader.Close();
				}
				finally
				{
					ReleaseLockObject(identifier);
				}
			}
		}

		private void DeleteFile(FileInfo file)
		{
			if (!file.Exists)
				return;
			try
			{
				long len = file.Length;
				file.Delete();
				diskSpace -= len;
			}
			catch
			{
			}
		}

		public void ClearPartialMatches(string partIdentifier)
		{
			lock (lockObjects)
			{
				List<CacheItemInfo> list = new List<CacheItemInfo>();
				SQLiteCommand cmd = new SQLiteCommand(sqlSelect, conn);
				cmd.Parameters.Add(new SQLiteParameter("@Identifier", partIdentifier));
				SQLiteDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
					list.Add(new CacheItemInfo(reader));
				reader.Close();

				cmd = new SQLiteCommand(sqlDeletePartialMatches, conn);
				cmd.Parameters.Add(new SQLiteParameter("@Identifier", partIdentifier));
				cmd.ExecuteNonQuery();

				foreach (CacheItemInfo info in list)
					DeleteFile(info.File);
			}
		}

		private CacheItemInfo ValidateCacheItem(CacheItemInfo info)
		{
			bool fileExists = File.Exists(info.PhysicalPath);
			SQLiteCommand cmd;
			if (info.ExpiryDate < SprocketDate.Now || !fileExists)
			{
				cmd = new SQLiteCommand(sqlDelete, conn);
				cmd.Parameters.Add(new SQLiteParameter("@Identifier", info.IdentifierString));
				cmd.ExecuteNonQuery();
				if (fileExists)
					try { DeleteFile(info.File); }
					catch { }
				return null;
			}
			if (!info.ForceExpiryAfterDuration && info.ExpiryDate != DateTime.MaxValue)
				info.ExpiryDate = SprocketDate.Now.Add(info.ExpiryDate - info.LastAccess);
			info.LastAccess = SprocketDate.Now;
			cmd = new SQLiteCommand(sqlUpdate, conn);
			cmd.Parameters.Add(new SQLiteParameter("@Identifier", info.IdentifierString));
			cmd.Parameters.Add(new SQLiteParameter("@LastAccess", info.LastAccess));
			cmd.Parameters.Add(new SQLiteParameter("@ExpiryDate", info.ExpiryDate));
			new Thread(new ParameterizedThreadStart(AccessThread)).Start(cmd);
			//cmd.ExecuteNonQuery();
			return info;
		}

		private void AccessThread(object data)
		{
			SQLiteCommand cmd = (SQLiteCommand)data;
			try
			{
				cmd.ExecuteNonQuery();
			}
			catch
			{
			}
		}

		private CacheItemInfo CheckDBForIdentifier(string identifier)
		{
			SQLiteCommand cmd = new SQLiteCommand(sqlSelect, conn);
			cmd.Parameters.Add(new SQLiteParameter("@Identifier", identifier));
			SQLiteDataReader reader = cmd.ExecuteReader();
			try
			{
				if (reader.Read())
					return ValidateCacheItem(new CacheItemInfo(reader));
				else
					return null;
			}
			finally
			{
				reader.Close();
			}
		}

		private CacheItemInfo CheckDBForSprocketPath(string sprocketPath)
		{
			SQLiteCommand cmd = new SQLiteCommand(sqlSelectAll, conn);
			cmd.Parameters.Add(new SQLiteParameter("@SprocketPath", sprocketPath));
			SQLiteDataReader reader = cmd.ExecuteReader();
			try
			{
				if (reader.Read())
					return ValidateCacheItem(new CacheItemInfo(reader));
				else
					return null;
			}
			finally
			{
				reader.Close();
			}
		}

		private CacheItemInfo FromIdentifier(string identifier)
		{
			CacheItemInfo info = null;
			bool memCached;

			// try to get the item from the memory cache
			lock (memoryCache)
				memCached = memoryCache.TryGetValue(identifier, out info);
			if (info != null)
				info = ValidateCacheItem(info);

			// if the item wasn't in the memory cache, check the db cache
			if (info == null)
			{
				info = CheckDBForIdentifier(identifier);
				if (info == null)
					return null;

				// put the item into the memory cache
				lock (memoryCache)
					memoryCache.Add(identifier, info);
				diskSpace += info.File.Length;
			}

			if (!File.Exists(info.PhysicalPath))
			{
				lock (memoryCache)
					memoryCache.Remove(identifier);
				return null;
			}

			return info;
		}

		public void Write(CacheItemInfo info, Stream stream)
		{
			lock (memoryCache)
				if (memoryCache.ContainsKey(info.IdentifierString))
				{
					CacheItemInfo c = memoryCache[info.IdentifierString];
					memoryCache.Remove(c.IdentifierString);
					DeleteFile(c.File);
				}

			if (info.SprocketPath != null)
				lock (sprocketPathMemoryCache)
					sprocketPathMemoryCache.Remove(info.SprocketPath);

			lock (GetLockObject(info.IdentifierString))
			{
				try
				{
					SQLiteCommand cmd = new SQLiteCommand(sqlInsert, conn);
					info.AddSqlParameters(cmd);
					cmd.ExecuteNonQuery();

					if (stream.CanSeek)
						stream.Seek(0, SeekOrigin.Begin);

					FileInfo file = info.File;
					if (!file.Directory.Exists)
						file.Directory.Create();
					DeleteFile(info.File);
					using (FileStream fs = File.Create(info.PhysicalPath))
					{
						byte[] buffer = new byte[100000];
						int n;
						do
						{
							n = stream.Read(buffer, 0, buffer.Length);
							if (n == 0) break;
							fs.Write(buffer, 0, n);
						} while (true);
						fs.Flush();
						fs.Close();
					}

					lock (memoryCache)
						memoryCache[info.IdentifierString] = info;
					file.Refresh();
					diskSpace += file.Length;
				}
				catch
				{
					DeleteFile(info.File);
					throw;
				}
				finally
				{
					ReleaseLockObject(info.IdentifierString);
				}
			}
		}

		public void WriteText(CacheItemInfo info, string text)
		{
			lock (memoryCache)
				if (memoryCache.ContainsKey(info.IdentifierString))
				{
					CacheItemInfo c = memoryCache[info.IdentifierString];
					memoryCache.Remove(c.IdentifierString);
					DeleteFile(c.File);
				}

			if(info.SprocketPath != null)
				lock (sprocketPathMemoryCache)
					sprocketPathMemoryCache.Remove(info.SprocketPath);

			lock (GetLockObject(info.IdentifierString))
			{
				try
				{
					FileInfo file = info.File;
					if (!file.Directory.Exists)
						file.Directory.Create();
					SQLiteCommand cmd = new SQLiteCommand(sqlInsert, conn);
					info.AddSqlParameters(cmd);
					cmd.ExecuteNonQuery();

					File.WriteAllText(info.PhysicalPath, text, Encoding.Unicode);

					lock (memoryCache)
						memoryCache[info.IdentifierString] = info;
					file.Refresh();
					diskSpace += file.Length;
				}
				catch
				{
					DeleteFile(info.File);
					throw;
				}
				finally
				{
					ReleaseLockObject(info.IdentifierString);
				}
			}
		}

		public Stream Read(string identifier)
		{
			lock (GetLockObject(identifier))
			{
				try
				{
					CacheItemInfo info = FromIdentifier(identifier);
					if (info == null)
						return null;
					MemoryStream stream = new MemoryStream(File.ReadAllBytes(info.PhysicalPath));
					stream.Seek(0, SeekOrigin.Begin);
					return stream;
				}
				finally
				{
					ReleaseLockObject(identifier);
				}
			}
		}

		public string ReadText(string identifier)
		{
			lock (GetLockObject(identifier))
			{
				try
				{
					CacheItemInfo info = FromIdentifier(identifier);
					if (info == null)
						return null;
					return File.ReadAllText(info.PhysicalPath, Encoding.Unicode);
				}
				finally
				{
					ReleaseLockObject(identifier);
				}
			}
		}

		public bool Transmit(HttpResponse response, string sprocketPath)
		{
			CacheItemInfo info = null;
			bool memCached;
			lock (sprocketPathMemoryCache)
				memCached = sprocketPathMemoryCache.TryGetValue(sprocketPath, out info);
			if (info != null)
				info = ValidateCacheItem(info);

			if (info == null)
			{
				info = CheckDBForSprocketPath(sprocketPath);
				if (info == null)
					return false;
				lock (sprocketPathMemoryCache)
					sprocketPathMemoryCache.Add(sprocketPath, info);
			}
			if (!File.Exists(info.PhysicalPath))
			{
				if (memCached)
					lock (sprocketPathMemoryCache)
						sprocketPathMemoryCache.Remove(sprocketPath);
				return false;
			}
			response.ContentType = info.ContentType;
			response.TransmitFile(info.PhysicalPath);
			return true;
		}
	}
}
