using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket;
using Sprocket.Security;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Security
{
	public partial class WebSecurity : IDataHandlerModule
	{
		public void ExecuteDataScripts()
		{
			string sql = ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Security.DatabaseScripts.sqlserver_data_001.sql");
			((SqlDatabase)Database.Main).ExecuteScript(sql);
		}
	}
}
