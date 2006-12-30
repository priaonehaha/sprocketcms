using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket.Web;

namespace Sprocket.Web.CMS.SprocketScript.Parser
{
	class ScriptTestModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(Instance_OnLoadRequestedPath);
		}

		void Instance_OnLoadRequestedPath(System.Web.HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (sprocketPath == "scripttest")
			{
				string html = Sprocket.Utility.ResourceLoader.LoadTextResource("Sprocket.Web.CMS.SprocketScript.test.htm");
				SprocketScript script = new SprocketScript(html);
				HttpContext.Current.Response.ContentType = "text/html";
				script.Execute(HttpContext.Current.Response.OutputStream);
				//string test = script.Execute();
				//HttpContext.Current.Response.Write(test);
				handled.Set();
			}
		}
	}
}
