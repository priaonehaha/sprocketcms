using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

using Sprocket;
using Sprocket.Web;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Pages
{
	public partial class PageRequestHandler
	{
		HttpRequest Request
		{
			get { return HttpContext.Current.Request; }
		}
		HttpResponse Response
		{
			get { return HttpContext.Current.Response; }
		}

		public static PageRequestHandler Instance
		{
			get { return (PageRequestHandler)Core.Instance[typeof(PageRequestHandler)].Module; }
		}

		private Dictionary<string, IPlaceHolderRenderer> placeHolderRenderers = new Dictionary<string, IPlaceHolderRenderer>();
		public delegate void RegisteringPlaceHolderRenderers(Dictionary<string, IPlaceHolderRenderer> placeHolderRenderers);
		public delegate void BeforeRenderPage(PageEntry page, string sprocketPath, string[] pathSections);
		public event RegisteringPlaceHolderRenderers OnRegisteringPlaceHolderRenderers;
		public event BeforeRenderPage OnBeforeRenderPage;

		internal Dictionary<string, IPlaceHolderRenderer> PlaceHolderRenderers
		{
			get { return placeHolderRenderers; }
		}
		private void RegisterPlaceHolderRenderers()
		{
			placeHolderRenderers.Add("name", new NamePlaceHolderRenderer());
			placeHolderRenderers.Add("xml", new XmlPlaceHolderRenderer());
			placeHolderRenderers.Add("externalxml", new ExternalXmlPlaceHolderRenderer());
			placeHolderRenderers.Add("embed", new EmbedPlaceHolderRenderer());
			placeHolderRenderers.Add("embedsecure", new EmbedSecurePlaceHolderRenderer());
			placeHolderRenderers.Add("template", new TemplatePlaceHolderRenderer());
			placeHolderRenderers.Add("templatesecure", new TemplateSecurePlaceHolderRenderer());
			placeHolderRenderers.Add("list", new ListPlaceHolderRenderer());
			placeHolderRenderers.Add("pageentry", new PageEntryPlaceHolderRenderer());
			placeHolderRenderers.Add("path", new PathPlaceHolderRenderer());
			placeHolderRenderers.Add("ajaxscripts", new AjaxScriptsPlaceHolderRenderer());
			if (OnRegisteringPlaceHolderRenderers != null)
				OnRegisteringPlaceHolderRenderers(placeHolderRenderers);
		}

		private Dictionary<string, IOutputFormatter> outputFormatters = new Dictionary<string, IOutputFormatter>();
		public delegate void RegisteringOutputFormatters(Dictionary<string, IOutputFormatter> outputFormatters);
		public event RegisteringOutputFormatters OnRegisteringOutputFormatters;
		internal Dictionary<string, IOutputFormatter> OutputFormatters
		{
			get { return outputFormatters; }
		}
		private void RegisterOutputFormatters()
		{
			outputFormatters.Add("", new DefaultOutputFormatter());
			outputFormatters.Add("DateTime", new DateTimeOutputFormatter());
			if (OnRegisteringOutputFormatters != null)
				OnRegisteringOutputFormatters(outputFormatters);
		}

		void OnLoadRequestedPath(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (handled.Handled) return;

			switch (sprocketPath)
			{
				case "$reset":
					PageRegistry.UpdateValues();
					TemplateRegistry.Reload();
					ListRegistry.Reload();
					OutputFormatRegistry.Reload();
					GeneralRegistry.Reload();
					ContentCache.ClearCache();
					WebUtility.Redirect("");
					break;

				default:
					PageRegistry.CheckDate();

					PageEntry page = PageRegistry.Pages.FromPath(sprocketPath);
					if(page == null)
						return;
					if (OnBeforeRenderPage != null)
						OnBeforeRenderPage(page, sprocketPath, pathSections);
					string output = page.Render();
					if (output == null)
						return;
					Response.Write(output);
					break;
			}

			handled.Set();
		}

		void OnPathNotFound(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (!sprocketPath.Contains(".")) return;
			string urlpath;
			if (pathSections.Length == 1)
				urlpath = "";
			else
				urlpath = sprocketPath.Substring(0, sprocketPath.Length - pathSections[pathSections.Length - 1].Length - 1);
			XmlElement node = (XmlElement)PagesXml.SelectSingleNode("//Page[@Path='" + urlpath + "']");
			if (node == null) return;
			string newurl = "resources/content/" + node.GetAttribute("ContentFile");
			newurl = WebUtility.BasePath + newurl.Substring(0, newurl.LastIndexOf('/') + 1) + pathSections[pathSections.Length - 1];
			if (!File.Exists(HttpContext.Current.Server.MapPath(newurl)))
				return;
			HttpContext.Current.Response.TransmitFile(HttpContext.Current.Server.MapPath(newurl));
			handled.Set();
		}

		public static Dictionary<string, XmlDocument> XmlCache
		{
			get
			{
				Dictionary<string, XmlDocument> xc;
				HttpApplicationState app = HttpContext.Current.Application;
				app.Lock();
				if (app["Sprocket_PGREQ_XmlCache"] == null)
				{
					xc = new Dictionary<string, XmlDocument>();
					app["Sprocket_PGREQ_XmlCache"] = xc;
				}
				else
					xc = (Dictionary<string, XmlDocument>)app["Sprocket_PGREQ_XmlCache"];
				app.UnLock();
				return xc;
			}
		}

		internal XmlDocument GetXmlDocument(string sprocketPath)
		{
			HttpContext.Current.Application.Lock();
			if (XmlCache.ContainsKey(sprocketPath))
			{
				HttpContext.Current.Application.UnLock();
				return XmlCache[sprocketPath];
			}
			XmlDocument doc = new XmlDocument();
			string path = WebUtility.MapPath(sprocketPath);
			if (!File.Exists(path))
				return null;
			doc.Load(path);
			XmlCache.Add(sprocketPath, doc);
			HttpContext.Current.Application.UnLock();
			return doc;
		}

		void OnBeginHttpRequest(HttpApplication appInst, HandleFlag handled)
		{
			HttpApplicationState app = HttpContext.Current.Application;
			app.Lock();
			if (app["Sprocket_PGREQ_XmlCache_Count"] == null)
				app["Sprocket_PGREQ_XmlCache_Count"] = 1;
			else
				app["Sprocket_PGREQ_XmlCache_Count"] = (int)app["Sprocket_PGREQ_XmlCache_Count"] + 1;
			app.UnLock();
		}

		void OnEndHttpRequest(HttpApplication appInst)
		{
			HttpApplicationState app = HttpContext.Current.Application;
			app.Lock();
			bool k = false;
			if (app["Sprocket_PGREQ_XmlCache_Count"] == null)
				k = true;
			else
			{
				app["Sprocket_PGREQ_XmlCache_Count"] = (int)app["Sprocket_PGREQ_XmlCache_Count"] - 1;
				if ((int)app["Sprocket_PGREQ_XmlCache_Count"] <= 0)
					k = true;
			}
			if (k)
			{
				app["Sprocket_PGREQ_XmlCache_Count"] = null;
				app["Sprocket_PGREQ_XmlCache"] = null;
			}
			app.UnLock();
		}

		XmlDocument PagesXml
		{
			get
			{
				XmlDocument pages = null;
				string path = WebUtility.MapPath("resources/definitions/pages.xml");
				if (!File.Exists(path))
					return null;

				FileInfo pgxmlfile = new FileInfo(path);
				HttpApplicationState app = HttpContext.Current.Application;
				app.Lock();
				if (app["PagesXmlModified"] != null && app["PagesXmlDocument"] != null)
					if ((DateTime)app["PagesXmlModified"] == pgxmlfile.LastWriteTime)
						pages = (XmlDocument)app["PagesXmlDocument"];
				if (pages == null)
				{
					pages = new XmlDocument();
					pages.Load(path);
					app["PagesXmlModified"] = pgxmlfile.LastWriteTime;
					app["PagesXmlDocument"] = pages;
				}
				app.UnLock();
				return pages;
			}
		}

		XmlDocument ListsXml
		{
			get
			{
				XmlDocument lists = null;
				string path = WebUtility.MapPath("resources/definitions/lists.xml");
				FileInfo xmlfile = new FileInfo(path);
				HttpApplicationState app = HttpContext.Current.Application;
				app.Lock();
				if (app["ListsXmlModified"] != null && app["ListsXmlDocument"] != null)
					if ((DateTime)app["ListsXmlModified"] == xmlfile.LastWriteTime)
						lists = (XmlDocument)app["ListsXmlDocument"];
				if (lists == null)
				{
					lists = new XmlDocument();
					lists.Load(path);
					app["ListsXmlModified"] = xmlfile.LastWriteTime;
					app["ListsXmlDocument"] = lists;
				}
				app.UnLock();
				return lists;
			}
		}
	}
}
