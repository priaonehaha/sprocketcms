using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

using Sprocket.SystemBase;
using Sprocket.Web;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Pages
{
	[ModuleDependency("WebEvents")]
	[ModuleDependency("DatabaseManager")]
	public partial class PageRequestHandler : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			if (!File.Exists(WebUtility.MapPath(PageRegistry.XmlFilePath))) return;
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(OnBeginHttpRequest);
			WebEvents.Instance.OnEndHttpRequest += new WebEvents.HttpApplicationEventHandler(OnEndHttpRequest);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
			WebEvents.Instance.OnPathNotFound += new WebEvents.RequestedPathEventHandler(OnPathNotFound);
			//WebsiteAdmin.Instance.OnAdminRequest += new WebsiteAdmin.AdminRequestHandler(OnAdminRequest);
		}

		public void Initialise(ModuleRegistry registry)
		{
			RegisterPlaceHolderRenderers();
			RegisterOutputFormatters();
		}

		public string RegistrationCode
		{
			get { return "PageRequestHandler"; }
		}

		public string Title
		{
			get { return "Web Page Request Handler"; }
		}

		public string ShortDescription
		{
			get { return "Handles requests website pages and performs other page-related tasks"; }
		}
	}
}
