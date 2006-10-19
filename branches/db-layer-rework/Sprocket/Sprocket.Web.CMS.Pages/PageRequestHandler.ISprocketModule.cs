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
	[ModuleDependency("WebEvents")]
	[ModuleDependency("DatabaseManager")]
	[ModuleDependency("SecurityProvider")]
	[ModuleDependency("FileManager")]
	[ModuleDescription("Handles requests website pages and performs other page-related tasks")]
	public partial class PageRequestHandler : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			Core.Instance.OnInitialise += new EmptyEventHandler(Instance_OnInitialise);
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(OnBeginHttpRequest);
			WebEvents.Instance.OnEndHttpRequest += new WebEvents.HttpApplicationEventHandler(OnEndHttpRequest);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
			WebEvents.Instance.OnPathNotFound += new WebEvents.RequestedPathEventHandler(OnPathNotFound);
			WebsiteAdmin.Instance.OnAdminRequest += new WebsiteAdmin.AdminRequestHandler(OnAdminRequest);
		}

		void Instance_OnInitialise()
		{
			RegisterPlaceHolderRenderers();
			RegisterOutputFormatters();
		}

		public string Title
		{
			get { return "Web Page Request Handler"; }
		}
	}
}
