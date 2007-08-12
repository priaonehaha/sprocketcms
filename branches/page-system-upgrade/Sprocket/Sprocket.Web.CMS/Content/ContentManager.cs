using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content
{
	[ModuleTitle("Content Manager")]
	[ModuleDescription("The content management engine that handles content, pages and the templates they use")]
	[ModuleDependency(typeof(WebEvents))]
	[AjaxMethodHandler("ContentManager")]
	public sealed class ContentManager : ISprocketModule
	{
		public static ContentManager Instance
		{
			get { return (ContentManager)Core.Instance[typeof(ContentManager)].Module; }
		}

		public delegate void BeforeRenderPage(PageEntry page);
		public event BeforeRenderPage OnBeforeRenderPage;

		private class StateValues
		{
			public string XmlSprocketPath = "resources/definitions.xml";
			public string XmlPath = null;
			public XmlDocument MainXml = null;
			public DateTime LastXmlFileUpdate = DateTime.MinValue;
			public TemplateRegistry Templates = null;
			public PageRegistry Pages = null;
			public Stack<PageEntry> PageStack = new Stack<PageEntry>();
			public Dictionary<string, List<PagePreprocessorHandler>> PagePreProcessors = new Dictionary<string,List<PagePreprocessorHandler>>();
		}
		
		private StateValues stateValues = new StateValues();
		private static StateValues Values
		{
			get { return Instance.stateValues; }
		}

		public static void AddPagePreprocessor(string pageCode, PagePreprocessorHandler method)
		{
			if (!Values.PagePreProcessors.ContainsKey(pageCode))
				Values.PagePreProcessors.Add(pageCode, new List<PagePreprocessorHandler>());
			Values.PagePreProcessors[pageCode].Add(method);
		}

		public static XmlDocument DefinitionsXml
		{
			get
			{
				lock (WebUtility.GetSyncObject("Sprocket.Web.CMS.Content.ContentManager.MainXml"))
				{
					if (Values.XmlPath == null)
						Values.XmlPath = WebUtility.MapPath(Values.XmlSprocketPath);
					if (!File.Exists(Values.XmlPath))
						using (StreamWriter sw = new StreamWriter(Values.XmlPath))
						{
							sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<Definitions>\r\n\t<Pages />\r\n\t<Templates />\r\n</Definitions>");
							sw.Flush();
							sw.Close();
						}
					if (IsDefinitionsXmlOutOfDate || Values.MainXml == null)
					{
						Values.Templates = null;
						Values.Pages = null;
						Values.MainXml = new XmlDocument();
						Values.MainXml.Load(Values.XmlPath);
						Values.LastXmlFileUpdate = new FileInfo(Values.XmlPath).LastWriteTime;
					}
				}
				return Values.MainXml;
			}
		}

		public static bool IsDefinitionsXmlOutOfDate
		{
			get
			{
				if (Values.XmlPath == null)
					return true;
				return new FileInfo(Values.XmlPath).LastWriteTime != Values.LastXmlFileUpdate;
			}
		}

		public static TemplateRegistry Templates
		{
			get
			{
				if (Values.Templates == null)
					Values.Templates = new TemplateRegistry((XmlElement)DefinitionsXml.SelectSingleNode("/Definitions/Templates"));
				return Values.Templates;
			}
		}

		public static PageRegistry Pages
		{
			get
			{
				if (Values.Pages == null)
					Values.Pages = new PageRegistry(Templates, DefinitionsXml.SelectSingleNode("/Definitions/Pages") as XmlElement);
				return Values.Pages;
			}
		}

		public static Stack<PageEntry> PageStack
		{
			get { return Values.PageStack; }
		}

		private PageEntry requestedPage = null;
		public static PageEntry RequestedPage
		{
			get { return Instance.requestedPage; }
			internal set { Instance.requestedPage = value; }
		}

		HttpRequest Request { get { return HttpContext.Current.Request; } }
		HttpResponse Response { get { return HttpContext.Current.Response; } }

		#region Module Event Handlers
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(WebEvents_OnBeginHttpRequest);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
			WebEvents.Instance.OnPathNotFound += new WebEvents.RequestedPathEventHandler(WebEvents_OnPathNotFound);
			WebEvents.Instance.OnEndHttpRequest += new WebEvents.HttpApplicationEventHandler(WebEvents_OnEndHttpRequest);
		}

		void WebEvents_OnBeginHttpRequest(HandleFlag handled)
		{
			RequestSpeedExpression.Set();
			Values.PageStack.Clear();
			if (IsDefinitionsXmlOutOfDate)
			{
				Values.Templates = null;
				Values.Pages = null;
			}
		}

		void WebEvents_OnPathNotFound(HandleFlag handled)
		{
			//#region Map missing referenced files (e.g. images and css) to the same location as the content file

			////if (!SprocketPath.Value.Contains("."))
			////{
			////    HttpContext.Current.Response.Write(ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Content.404.htm"));
			////    handled.Set();
			////    return;
			////}
			//string urlpath;
			//if (SprocketPath.Sections.Length == 1)
			//    urlpath = "";
			//else
			//    urlpath = SprocketPath.Value.Substring(0, SprocketPath.Value.Length - SprocketPath.Sections[SprocketPath.Sections.Length - 1].Length - 1);

			//PageEntry page = Pages.FromPath(urlpath);
			//if (page == null) return;
			//string newurl = page.ContentFile;
			//newurl = WebUtility.BasePath + newurl.Substring(0, newurl.LastIndexOf('/') + 1) + SprocketPath.Sections[SprocketPath.Sections.Length - 1];
			//if (!File.Exists(HttpContext.Current.Server.MapPath(newurl)))
			//    return;
			//HttpContext.Current.Response.TransmitFile(HttpContext.Current.Server.MapPath(newurl));
			//handled.Set();

			//#endregion
		}

		void WebEvents_OnLoadRequestedPath(HandleFlag handled)
		{
			requestedPage = null;
			if (handled.Handled) return;
			PageEntry page = Pages.FromPath(SprocketPath.Value);
			if (page == null)
				return;
			requestedPage = page;
			if (Values.PagePreProcessors.ContainsKey(page.PageCode))
				foreach (PagePreprocessorHandler method in Values.PagePreProcessors[page.PageCode])
					method(page);
			if (OnBeforeRenderPage != null)
				OnBeforeRenderPage(page);
			string txt = page.Render();
			Response.ContentType = page.ContentType;
			Response.Write(txt);
			handled.Set();
		}

		void WebEvents_OnEndHttpRequest()
		{
			PageStack.Clear();
			requestedPage = null;
		}
		#endregion

		/// <summary>
		/// Indicates the descendent sprocket path for the current page request. This is only relevant for pages that
		/// have HandleSubPaths set to True, and returns the SprocketPath section of the page's path that is beyond
		/// the base path for that page. For example, if the path for the page is "content/news" and the actual
		/// current sprocket path is "content/news/2006/june/7", the descendent path returned is "2006/june/7". If
		/// there is no descendent path, or the current path does not map to a registered PageEntry object,
		/// String.Empty is returned.
		/// </summary>
		public static string DescendentPath
		{
			get
			{
				if (CurrentRequest.Value["ContentManager_DescendentPath_Value"] != null)
					return (string)CurrentRequest.Value["ContentManager_DescendentPath_Value"];
				string path;
				if (RequestedPage == null)
					path = String.Empty;
				else
					path = SprocketPath.GetDescendentPath(RequestedPage.Path);
				CurrentRequest.Value["ContentManager_DescendentPath_Value"] = path;
				return path;
			}
		}
	}

	public delegate void PagePreprocessorHandler(PageEntry page);
}
