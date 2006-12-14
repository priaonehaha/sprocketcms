using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Data.SqlClient;

using Sprocket;
using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Web
{
	[AjaxMethodHandler("TestModule")]
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDescription("A module for writing test code.")]
	[ModuleTitle("Testing Module")]
	public class TestModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
		}

		void OnLoadRequestedPath(HttpApplication app, string path, string[] pathSections, HandleFlag handled)
		{
			switch (path)
			{
				default:
					return;
			}
			handled.Set();

			//DatabaseManager.DatabaseEngine.PersistConnection();
			//SqlCommand cmd;
			//using (SqlConnection conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection())
			//{
			//    conn.Open();
			//    cmd = new SqlCommand("select @@version", conn);
			//    cmd.ExecuteNonQuery();
			//}
			//cmd = new SqlCommand("select @@version", (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection());
			//cmd.ExecuteNonQuery();
			//DatabaseManager.DatabaseEngine.ReleaseConnection();

			//HttpContext.Current.Response.Write("All good.");
		}

		//[AjaxMethod()]
		public string[] MethodTest(string x, int y, string[] z)
		{
			return new string[] { "{x:" + x + "}", "{y:" + y + "}", "{z.length:" + z.Length + "}" };
		}
	}
}
