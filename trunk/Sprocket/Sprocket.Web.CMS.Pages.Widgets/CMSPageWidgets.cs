using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Utility;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.SystemBase;

namespace Sprocket.Web.CMS.Pages.Widgets
{
	public class CMSPageWidgets : ISprocketModule, IDataHandlerModule
	{
		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			PageRequestHandler.Instance.OnRegisteringPlaceHolderRenderers += new PageRequestHandler.RegisteringPlaceHolderRenderers(OnRegisteringPlaceHolderRenderers);
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "CMSPageWidgets"; }
		}

		public string Title
		{
			get { return "CMS Page Widgets"; }
		}

		public string ShortDescription
		{
			get { return "Contains various renderers and formatters for use with the SprocketCMS Pages module."; }
		}

		#endregion

		#region IDataHandlerModule Members

		public void ExecuteDataScripts(DatabaseEngine engine)
		{
			if (engine != DatabaseEngine.SqlServer)
				return;

			//((SqlDatabase)Database.Main).ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Pages.Widgets.SqlScripts.newsletter.sql"));
		}

		public void DeleteDatabaseStructure(DatabaseEngine engine)
		{
		}

		public bool SupportsDatabaseEngine(DatabaseEngine engine)
		{
			return engine == DatabaseEngine.SqlServer;
		}

		#endregion

		void OnRegisteringPlaceHolderRenderers(Dictionary<string, IPlaceHolderRenderer> placeHolderRenderers)
		{
			placeHolderRenderers.Add("basicscripts", new BasicScriptsPlaceHolderRenderer());
		}
	}
}
