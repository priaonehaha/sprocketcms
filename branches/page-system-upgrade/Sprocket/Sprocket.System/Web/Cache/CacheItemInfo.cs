using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;
using System.Data.SQLite;

using Sprocket;
using Sprocket.Web;

namespace Sprocket.Web.Cache
{
	internal class CacheItemInfo
	{
		private string identifierString = "";
		private DateTime lastAccess = SprocketDate.Now;
		private DateTime expiryDate = DateTime.MaxValue;
		private DateTime createDate = SprocketDate.Now;
		private bool preventExpiryDateChange = false;
		private string sprocketPath = "";
		private string contentType = "text/html";
		private string physicalPath;

		private FileInfo fileInfo = null;
		public FileInfo File
		{
			get
			{
				if (fileInfo == null)
					fileInfo = new FileInfo(PhysicalPath);
				return fileInfo;
			}
		}

		#region Properties

		public string PhysicalPath
		{
			get { return physicalPath; }
		}

		/// <summary>
		/// Uniquely identifies this cache item. Care should be taken when choosing this value in order to
		/// prevent conflicts with other modules using the caching mechanism.
		/// </summary>
		public string IdentifierString
		{
			get { return identifierString; }
		}

		/// <summary>
		/// Specifies the content type to use when direct URL requests are made for this cached item.
		/// </summary>
		public string ContentType
		{
			get { return contentType; }
			set { contentType = value; }
		}

		/// <summary>
		/// Specifies an optional URL to use to serve this item directly.
		/// </summary>
		public string SprocketPath
		{
			get { return sprocketPath; }
			set { sprocketPath = value; }
		}

		/// <summary>
		/// When a cache item is requested, its expiry date is shifted back to prevent frequently-requested
		/// items from expiring. PreventExpiryDateChange prevents this from happening, thus forcing the cached
		/// item to expire at the originally-specified time. Not applicable if ExpiryTime is null.
		/// </summary>
		public bool ForceExpiryAfterDuration
		{
			get { return preventExpiryDateChange; }
			set { preventExpiryDateChange = value; }
		}

		/// <summary>
		/// The original date/time that the cache item was created
		/// </summary>
		public DateTime CreateDate
		{
			get { return createDate; }
		}

		/// <summary>
		/// The date/time that this item should expire if not accessed. This time is shifted back a proportional
		/// amount when the cache item is accessed unless PreventExpiryDateChange is true.
		/// </summary>
		public DateTime ExpiryDate
		{
			get { return expiryDate; }
			set { expiryDate = value; }
		}

		/// <summary>
		/// Specifies the most recent time the cache item was accessed. Used to calculate how far back to shift
		/// ExpiryDate when a cache item is accessed.
		/// </summary>
		public DateTime LastAccess
		{
			get { return lastAccess; }
			set { lastAccess = value; }
		}
		#endregion

		private void CalculatePhysicalPath()
		{
			byte[] bytes = Utility.Crypto.CalculateMD5Hash(identifierString);
			physicalPath = WebUtility.MapPath("datastore/content-cache/" + bytes[15] + "/" + new Guid(bytes).ToString("N") + ".file");
		}

		public CacheItemInfo(string identifierString, TimeSpan? expiresAfter, bool forceExpiryAfterDuration, string sprocketPath, string contentType)
		{
			if (identifierString == null || identifierString == "")
				throw new NullReferenceException("Expected a non-null, non-zero-length value for the cache item identifier string");
			this.identifierString = identifierString;
			if (expiresAfter.HasValue)
				expiryDate = createDate.Add(expiresAfter.Value);
			else
				expiryDate = DateTime.MaxValue;
			this.contentType = contentType;
			this.sprocketPath = sprocketPath;
			this.preventExpiryDateChange = forceExpiryAfterDuration;
			CalculatePhysicalPath();
		}

		public CacheItemInfo(IDataReader reader) 
		{
			identifierString = (string)reader["Identifier"];
			physicalPath = (string)reader["Path"];
			lastAccess = (DateTime)reader["LastAccess"];
			expiryDate = (DateTime)reader["ExpiryDate"];
			createDate = (DateTime)reader["CreateDate"];
			sprocketPath = reader["SprocketPath"] == DBNull.Value ? null : (string)reader["SprocketPath"];
			contentType = reader["ContentType"] == DBNull.Value ? null : (string)reader["ContentType"];
			preventExpiryDateChange = (bool)reader["PreventExpiryDateChange"];
			CalculatePhysicalPath();
		}

		public void AddSqlParameters(SQLiteCommand cmd)
		{
			cmd.Parameters.Add(new SQLiteParameter("@Identifier", identifierString));
			cmd.Parameters.Add(new SQLiteParameter("@LastAccess", lastAccess));
			cmd.Parameters.Add(new SQLiteParameter("@ExpiryDate", expiryDate));
			cmd.Parameters.Add(new SQLiteParameter("@CreateDate", createDate));
			cmd.Parameters.Add(new SQLiteParameter("@PreventExpiryDateChange", preventExpiryDateChange));
			cmd.Parameters.Add(new SQLiteParameter("@SprocketPath", sprocketPath == null ? DBNull.Value : (object)sprocketPath));
			cmd.Parameters.Add(new SQLiteParameter("@ContentType", contentType == null ? DBNull.Value : (object)contentType));
			cmd.Parameters.Add(new SQLiteParameter("@Path", physicalPath));
		}

		public CacheItemInfo(FileStream fs)
		{
			Read(fs);
		}
		public void Write(FileStream fs)
		{
			WriteInt64(lastAccess.Ticks, fs);
			WriteInt64(expiryDate.Ticks, fs);
			WriteInt64(createDate.Ticks, fs);
			WriteBool(preventExpiryDateChange, fs);
			WriteString(sprocketPath, fs);
			WriteString(contentType, fs);
			WriteString(identifierString, fs);
		}
		private void Read(FileStream fs)
		{
			lastAccess = new DateTime(ReadInt64(fs));
			expiryDate = new DateTime(ReadInt64(fs));
			createDate = new DateTime(ReadInt64(fs));
			preventExpiryDateChange = ReadBool(fs);
			sprocketPath = ReadString(fs);
			contentType = ReadString(fs);
			identifierString = ReadString(fs);
			//fileName = new Guid(Utility.Crypto.CalculateMD5Hash(identifierString)).ToString("N");
		}
		private void WriteString(string value, FileStream fs)
		{
			if (value == null)
			{
				WriteInt32(0, fs);
				return;
			}
			WriteInt32(value.Length, fs);
			byte[] buffer = Encoding.Unicode.GetBytes(value);
			fs.Write(buffer, 0, buffer.Length);
		}
		private void WriteInt32(int value, FileStream fs)
		{
			fs.Write(BitConverter.GetBytes(value), 0, 4);
		}
		private void WriteInt64(long value, FileStream fs)
		{
			fs.Write(BitConverter.GetBytes(value), 0, 8);
		}
		private void WriteBool(bool value, FileStream fs)
		{
			fs.Write(BitConverter.GetBytes(value), 0, 1);
		}
		private long ReadInt64(FileStream fs)
		{
			return BitConverter.ToInt64(ReadBytes(8, fs), 0);
		}
		private int ReadInt32(FileStream fs)
		{
			return BitConverter.ToInt32(ReadBytes(4, fs), 0);
		}
		private bool ReadBool(FileStream fs)
		{
			return BitConverter.ToBoolean(ReadBytes(1, fs), 0);
		}
		private string ReadString(FileStream fs)
		{
			int len = ReadInt32(fs);
			return Encoding.Unicode.GetString(ReadBytes(len, fs));
		}
		private byte[] ReadBytes(int amount, FileStream fs)
		{
			byte[] buffer = new byte[amount];
			fs.Read(buffer, 0, amount);
			return buffer;
		}
	}
}
