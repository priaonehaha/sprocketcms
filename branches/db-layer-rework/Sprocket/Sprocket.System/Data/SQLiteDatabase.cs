using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;
using System.Transactions;

namespace Sprocket.Data
{
	public class SQLiteDatabase : IDatabaseHandler
	{
		public DbConnection CreateDefaultConnection()
		{
			return new SQLiteConnection("Data Source=" + PhysicalPath + ";UseUTF16Encoding=True;");
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
				else
					SQLiteConnection.CompressFile(PhysicalPath);
			}
			catch (Exception ex)
			{
				return new Result("Unable to initialise the default SQLite database: " + ex.Message);
			}

			Result result = new Result();
			using (TransactionScope scope = new TransactionScope())
			{
				if (OnInitialise != null)
					OnInitialise(result);
				if (result.Succeeded)
					scope.Complete();
			}
			return result;
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

		public string Title
		{
			get { return "SQLite 3"; }
		}

		public event InterruptableEventHandler OnInitialise;
	}
}
