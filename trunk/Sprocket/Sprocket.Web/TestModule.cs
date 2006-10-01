using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Sprocket;
using Sprocket.SystemBase;
using Sprocket.Utility;

namespace Sprocket.Web
{
	[AjaxMethodHandler()]
	[ModuleDependency("WebEvents")]
	class TestModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
		}

		void OnLoadRequestedPath(HttpApplication app, string path, string[] pathSections, HandleFlag handled)
		{
			if (path != "test")
				return;
			handled.Set();

			HttpContext c = HttpContext.Current;
			c.Response.Write("QS Keys:<br/>");
			for (int i = 0; i < c.Request.QueryString.Count; i++)
				HttpContext.Current.Response.Write(c.Request.QueryString.GetKey(i) + " = " + c.Request.QueryString[i] + "<br/>");

			c.Response.Write("QS Form:<br/>");
			for (int i = 0; i < c.Request.QueryString.Count; i++)
				HttpContext.Current.Response.Write(c.Request.Form.GetKey(i) + " = " + c.Request.Form[i] + "<br/>");

			string html = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + WebUtility.BasePath;
			HttpContext.Current.Response.Write(html);
			//string scripts = ((WebClientScripts)SystemCore.Instance["WebClientScripts"]).BuildScriptTags();
			//HttpContext.Current.Response.Write(scripts + html.Replace(Environment.NewLine, "<br />"));
		}

		[AjaxMethod()]
		public string[] MethodTest(string x, int y, string[] z)
		{
			return new string[] { "{x:" + x + "}", "{y:" + y + "}", "{z.length:" + z.Length + "}" };
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "TestModule"; }
		}

		public string Title
		{
			get { return "Testing Module"; }
		}

		public string ShortDescription
		{
			get { return "A module for writing test code."; }
		}
	}
}
