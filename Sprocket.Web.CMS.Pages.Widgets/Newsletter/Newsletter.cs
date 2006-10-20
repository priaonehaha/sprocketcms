using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using Sprocket.Utility;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.SystemBase;

namespace Sprocket.Web.CMS.Pages.Widgets
{
	[AjaxMethodHandler]
	public class NewsletterModule : ISprocketModule, IDataHandlerModule
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
			get { return "NewsletterModule"; }
		}

		public string Title
		{
			get { return "Newsletter Module"; }
		}

		public string ShortDescription
		{
			get { return "Provides functionality for having a newsletter subscription on the site."; }
		}

		#endregion

		#region IDataHandlerModule Members

		public void ExecuteDataScripts(DatabaseEngine engine)
		{
			if (engine != DatabaseEngine.SqlServer)
				return;

			((SqlDatabase)Database.Main).ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Pages.Widgets.Newsletter.newsletter.sql"));
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
			placeHolderRenderers.Add("newsletter", new NewsletterHandler());
		}

		[AjaxMethod]
		public void Subscribe(string email)
		{
			if (!Utilities.Validator.IsEmailAddress(email))
				return;
			Database.Main.RememberOpenState();
			IDbCommand cmd = Database.Main.CreateCommand("@Newsletter_Subscribe", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@EmailAddress", email);
			cmd.ExecuteNonQuery();
			Database.Main.CloseIfWasntOpen();
		}

		[AjaxMethod]
		public void Unsubscribe(string email)
		{
			if (!Utilities.Validator.IsEmailAddress(email))
				return;
			Database.Main.RememberOpenState();
			IDbCommand cmd = Database.Main.CreateCommand("@Newsletter_Unsubscribe", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@EmailAddress", email);
			cmd.ExecuteNonQuery();
			Database.Main.CloseIfWasntOpen();
		}
	}
}
