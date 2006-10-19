using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket.Web;
using Sprocket.Data;
using Sprocket;

namespace Sprocket.Web.DatabaseSetup
{
	[ModuleDependency("DatabaseManager")]
	[ModuleDependency("WebEvents")]
	[ModuleDescription("Provides a web interface for database setip and initialisation")]
	public class DatabaseSetup : ISprocketModule
	{
		void Instance_OnLoadRequestedPath(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (handled.Handled) return;
			if (sprocketPath == "$dbsetup")
			{
				DatabaseManager.Instance.ExecuteAllDataScripts(Database.Main.DatabaseEngine);
				HttpContext.Current.Response.Write("<p>Database setup completed.</p>");
				handled.Set();
			}
		}

		public static DatabaseSetup Instance
		{
			get { return (DatabaseSetup)Core.Instance["DatabaseSetup"]; }
		}

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(Instance_OnLoadRequestedPath);
		}

		public string Title
		{
			get { return "Database Setup Interface"; }
		}

		#endregion
	}
}
