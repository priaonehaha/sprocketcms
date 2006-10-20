using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;

namespace Sprocket.Data
{
	public class SQLiteDatabase : IDatabaseHandler
	{
		public DbConnection CreateDefaultConnection()
		{
			return new SQLiteConnection("Data Source=" + PhysicalPath);
		}

		public Result Initialise()
		{
			try
			{
				if (!File.Exists(PhysicalPath))
				{
					FileInfo info = new FileInfo(PhysicalPath);
					if (!info.Directory.Exists)
						info.Directory.Create();
					SQLiteConnection.CreateFile(PhysicalPath);
				}
			}
			catch (Exception ex)
			{
				return new Result("Unable to initialise the default SQLite database: " + ex.Message);
			}
			return new Result();
		}

		private string physicalPath = null;
		private string PhysicalPath
		{
			get
			{
				if (physicalPath == null)
					physicalPath = Web.WebUtility.MapPath("datastore/databases/main.sdb");
				return physicalPath;
			}
		}

		public Result CheckConfiguration()
		{
			return new Result();
		}
	}
}
