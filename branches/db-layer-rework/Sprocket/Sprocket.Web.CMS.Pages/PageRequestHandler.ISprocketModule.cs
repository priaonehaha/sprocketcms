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
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(DatabaseManager))]
	//[ModuleDependency(typeof(SecurityProvider))]
	//[ModuleDependency(typeof(FileManager))]
	[ModuleDescription("Handles requests website pages and performs other page-related tasks")]
	[ModuleTitle("Web Page Request Handler")]
	public partial class PageRequestHandler : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			Core.Instance.OnInitialise += new ModuleInitialisationHandler(Instance_OnInitialise);
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(OnBeginHttpRequest);
			WebEvents.Instance.OnEndHttpRequest += new WebEvents.HttpApplicationEventHandler(OnEndHttpRequest);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
			WebEvents.Instance.OnPathNotFound += new WebEvents.RequestedPathEventHandler(OnPathNotFound);
			WebsiteAdmin.Instance.OnAdminRequest += new WebsiteAdmin.AdminRequestHandler(OnAdminRequest);
		}

		void Instance_OnInitialise(Dictionary<Type, List<Type>> interfaceImplementations)
		{
			RegisterPlaceHolderRenderers();
			RegisterOutputFormatters();
		}
	}
}
