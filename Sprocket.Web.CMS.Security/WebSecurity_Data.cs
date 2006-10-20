using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.SystemBase;
using Sprocket.Security;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Security
{
	public partial class WebSecurity : IDataHandlerModule
	{
		public void ExecuteDataScripts(DatabaseEngine engine)
		{
			string sql = ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Security.DatabaseScripts.sqlserver_data_001.sql");
			((SqlDatabase)Database.Main).ExecuteScript(sql);
		}

		public void DeleteDatabaseStructure(DatabaseEngine engine)
		{
		}

		public bool SupportsDatabaseEngine(DatabaseEngine engine)
		{
			return engine == DatabaseEngine.SqlServer;
		}
	}
}
