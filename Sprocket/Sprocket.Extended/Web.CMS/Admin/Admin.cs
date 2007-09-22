using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Xml;

using Sprocket;
using Sprocket.Web;
using Sprocket.Web.CMS.Content;
using Sprocket.Web.Cache;
using Sprocket.Utility;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Admin
{
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(AjaxRequestHandler))]
	[ModuleDependency(typeof(SprocketSettings))]
	[ModuleDependency(typeof(WebAuthentication))]
	[ModuleDescription("The base platform upon which the Sprocket CMS web interface is built. Most modules for the CMS plug into this module.")]
	[ModuleTitle("Website Administration Module")]
	public partial class AdminHandler : ISprocketModule
	{
		public event AdminRequestHandler OnLoadAdminPage;
		public delegate void AdminRequestHandler(AdminInterface admin, PageEntry page, HandleFlag handled);
		public delegate string WebsiteNameHandler();

		private WebsiteNameHandler getWebsiteName;
		public WebsiteNameHandler GetWebsiteName
		{
			get { return getWebsiteName; }
			set { getWebsiteName = value; }
		}

		List<XmlSourceFileDependent> definitionsFiles = new List<XmlSourceFileDependent>();
		TemplateRegistry templates = null;
		PageRegistry pages = null;
		Dictionary<string, List<PagePreprocessorHandler>> pagePreProcessors = new Dictionary<string, List<PagePreprocessorHandler>>();

		HttpRequest Request { get { return HttpContext.Current.Request; } }
		HttpResponse Response { get { return HttpContext.Current.Response; } }

		public static AdminHandler Instance
		{
			get { return (AdminHandler)Core.Instance[typeof(AdminHandler)].Module; }
		}

		public TemplateRegistry Templates
		{
			get { return templates; }
		}

		private string GetDefaultWebsiteName()
		{
			return "SprocketCMS";
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			getWebsiteName = GetDefaultWebsiteName;

			Core.Instance.OnInitialiseComplete += new EmptyHandler(LoadDefinitionFiles);
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(WebEvents_OnBeginHttpRequest);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
			WebEvents.Instance.OnRequestedPathProcessed += new WebEvents.HttpApplicationEventHandler(WebEvents_OnRequestedPathProcessed);
			WebEvents.Instance.OnEndHttpRequest += new WebEvents.HttpApplicationEventHandler(WebEvents_OnEndHttpRequest);

			AddPagePreprocessor("Login", PreProcessLoginPage);
			AddPagePreprocessor("Logout", PreProcessLogout);
			WebEvents.AddFormProcessor(new WebEvents.FormPostAction("admin/login", null, null, null, ProcessLoginForm));
		}

		void WebEvents_OnBeginHttpRequest(HandleFlag handled)
		{
			if (IsAdminRequest && !AjaxRequestHandler.IsAjaxRequest)
			{
				foreach(XmlSourceFileDependent file in definitionsFiles)
					if (file.HasFileChanged)
					{
						definitionsFiles = new List<XmlSourceFileDependent>();
						LoadDefinitionFiles();
						break;
					}
			}
		}

		void WebEvents_OnLoadRequestedPath(HandleFlag handled)
		{
			if (handled.Handled) return;
			if (!IsAdminRequest) return;

			PageEntry page = pages.FromPath(SprocketPath.Value);
			if (page == null)
				return;

			KeyValuePair<string, object>[] vars;
			if (!SprocketPath.StartsWith("admin", "login"))
			{
				if (!WebAuthentication.VerifyAccess(PermissionType.AccessAdminArea))
				{
					WebUtility.Redirect("admin/login");
					return;
				}

				AdminInterface admin = new AdminInterface();
				WebClientScripts scripts = WebClientScripts.Instance;
				admin.AddMainMenuLink(new AdminMenuLink("Website Home", WebUtility.MakeFullPath(""), ObjectRank.Last, "website_home"));
				admin.AddMainMenuLink(new AdminMenuLink("Overview", WebUtility.MakeFullPath("admin"), ObjectRank.First, "website_overview"));
				admin.AddMainMenuLink(new AdminMenuLink("Log Out", WebUtility.MakeFullPath("admin/logout"), ObjectRank.Last, "log_out"));

				admin.AddFooterLink(new AdminMenuLink("Log Out", WebUtility.MakeFullPath("admin/logout"), ObjectRank.Early));
				admin.AddFooterLink(new AdminMenuLink("&copy; 2005-" + DateTime.UtcNow.Year + " " + SprocketSettings.GetValue("WebsiteName"), "", ObjectRank.Late));
				admin.AddFooterLink(new AdminMenuLink("Powered by Sprocket", "http://www.sprocketcms.com", ObjectRank.Last));
				admin.AddHeadSection(new AdminSection(scripts.BuildStandardScriptsBlock(), ObjectRank.Late));
				admin.WebsiteName = GetWebsiteName();

				if (OnLoadAdminPage != null)
				{
					OnLoadAdminPage(admin, page, handled);
					if (handled.Handled)
						return;
				}

				vars = admin.GetScriptVariables();
			}
			else
			{
				vars = new KeyValuePair<string, object>[1];
				vars[0] = new KeyValuePair<string, object>("_admin_websitename", GetWebsiteName());
			}
			
			ContentManager.RequestedPage = page;
			if (pagePreProcessors.ContainsKey(page.PageCode))
				foreach (PagePreprocessorHandler method in pagePreProcessors[page.PageCode])
					method(page);
			string txt = page.Render(vars);
			Response.ContentType = page.ContentType;
			Response.Write(txt);
			handled.Set();
		}

		void LoadDefinitionFiles()
		{
			templates = new TemplateRegistry();
			pages = new PageRegistry(templates, "admin");
			string dirpath = WebUtility.MapPath("resources/admin");
			if(!Directory.Exists(dirpath))
				Directory.CreateDirectory(dirpath);
			foreach (string dir in Directory.GetDirectories(dirpath))
			{
				string path = dir + "\\definitions.xml";
				if (File.Exists(path))
				{
					XmlSourceFileDependent file = new XmlSourceFileDependent(path);
					definitionsFiles.Add(file);
					XmlElement xml = file.Data.SelectSingleNode("/Definitions") as XmlElement;
					if (xml == null)
						continue;
					xml = file.Data.SelectSingleNode("/Definitions/Templates") as XmlElement;
					if (xml != null)
						templates.Load(xml);
					xml = file.Data.SelectSingleNode("/Definitions/Pages") as XmlElement;
					if (xml != null)
						pages.Load(xml);
				}
			}
		}

		/// <summary>
		/// Determines if the requested path is within the admin area, i.e. if the first path section is "admin".
		/// </summary>
		public static bool IsAdminRequest
		{
			get { return SprocketPath.Sections[0] == "admin"; }
		}

		public enum PermissionType
		{
			AccessAdminArea = 0
		}

		public static void AddPagePreprocessor(string pageCode, PagePreprocessorHandler method)
		{
			AdminHandler admin = Instance;
			if (!admin.pagePreProcessors.ContainsKey(pageCode))
				admin.pagePreProcessors.Add(pageCode, new List<PagePreprocessorHandler>());
			admin.pagePreProcessors[pageCode].Add(method);
		}

		void WebEvents_OnRequestedPathProcessed()
		{
			if (!HttpContext.Current.Response.ContentType.Contains("html"))
				return;
			if (IsAdminRequest)
				return;
			if (!WebAuthentication.VerifyAccess(PermissionType.AccessAdminArea))
				return;
			string html = WebUtility.CacheTextFile("resources/admin/header.htm");
			HttpContext.Current.Response.Write(string.Format(html, WebUtility.BasePath));
		}

		void WebEvents_OnEndHttpRequest()
		{
		}

		#region Login/Logout
		public void PreProcessLoginPage(PageEntry page)
		{
			if(WebAuthentication.VerifyAccess(PermissionType.AccessAdminArea))
				WebUtility.Redirect("admin");
		}

		public void ProcessLoginForm()
		{
			WebAuthentication auth = WebAuthentication.Instance;
			if (auth.ProcessLoginForm("SprocketUsername", "SprocketPassword", "SprocketPreserveLogin"))
				if (WebAuthentication.VerifyAccess(PermissionType.AccessAdminArea))
					WebUtility.Redirect("admin");
				else
					auth.ClearAuthenticationCookie();
			FormValues.Set("login", "", null, true);
		}

		public void PreProcessLogout(PageEntry page)
		{
			WebAuthentication.Instance.ClearAuthenticationCookie();
			WebUtility.Redirect("admin/logout");
		}

		#endregion
	}
}
