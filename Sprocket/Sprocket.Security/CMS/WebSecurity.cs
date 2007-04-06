using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Mail;

using Sprocket;
using Sprocket.Data;
using Sprocket.Security;
using Sprocket.Utility;
using Sprocket.Web;
using Sprocket.Web.Controls;
using Sprocket.Web.CMS.Admin;

namespace Sprocket.Web.CMS.Security
{
	[ModuleDependency(typeof(SprocketSettings))]
	[ModuleDependency(typeof(SecurityProvider))]
	[ModuleDependency(typeof(WebsiteAdmin))]
	[ModuleTitle("Security CMS Interface")]
	[ModuleDescription("The CMS interface for managing users and roles provided by the SecurityProvider module.")]
	public partial class WebSecurity : ISprocketModule
	{
		private static ClientSpace defaultClient = null;
		public static ClientSpace CurrentClientSpace
		{
			get
			{
				if (defaultClient == null)
					defaultClient = SecurityProvider.Instance.DataLayer.SelectClientSpace(SecurityProvider.ClientSpaceID);
				return defaultClient;
			}
		}

		public static WebSecurity Instance
		{
			get { return (WebSecurity)Core.Instance[typeof(WebSecurity)].Module; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(OnCheckingSettings);
			WebsiteAdmin.Instance.OnAdminRequest += new WebsiteAdmin.AdminRequestHandler(OnAdminRequest);
			WebsiteAdmin.Instance.OnCMSAdminAuthenticationSuccess += new InterruptableEventHandler<string>(OnCMSAdminAuthenticationSuccess);
			AdminWindow.Instance.OnBeforeDisplayAdminWindowOverlay += new InterruptableEventHandler(AdminWindow_OnBeforeDisplayAdminWindowOverlay);
			AjaxFormHandler.Instance.OnValidateField += new AjaxFormFieldValidationHandler(OnValidateField);
			AjaxFormHandler.Instance.OnValidateForm += new AjaxFormSubmissionHandler(OnValidateForm);
			AjaxFormHandler.Instance.OnSaveForm += new AjaxFormSubmissionHandler(OnSaveForm);
			AjaxRequestHandler.Instance.OnAjaxRequestAuthenticationCheck += new InterruptableEventHandler<System.Reflection.MethodInfo>(OnAjaxRequestAuthenticationCheck);
			Pages.PageRequestHandler.Instance.OnRegisteringPlaceHolderRenderers += new Sprocket.Web.CMS.Pages.PageRequestHandler.RegisteringPlaceHolderRenderers(Instance_OnRegisteringPlaceHolderRenderers);
		}

		void AdminWindow_OnBeforeDisplayAdminWindowOverlay(Result result)
		{
			if(WebAuthentication.Instance.IsLoggedIn)
				if(SecurityProvider.CurrentUser.HasPermission(PermissionType.AdministrativeAccess))
					return;
			result.SetFailed("access denied");
		}

		void Instance_OnRegisteringPlaceHolderRenderers(Dictionary<string, Sprocket.Web.CMS.Pages.IPlaceHolderRenderer> placeHolderRenderers)
		{
			placeHolderRenderers.Add("currentuser", new CurrentUserPlaceHolderRenderer());
		}

		void OnCMSAdminAuthenticationSuccess(string source, Result result)
		{
			if (!SecurityProvider.CurrentUser.HasPermission(PermissionType.AdministrativeAccess))
				result.SetFailed("You don't have access to this area.");
		}

		void OnAdminRequest(AdminInterface admin, HandleFlag handled)
		{
			// build the "current user" block
			User user = User.Select(SecurityProvider.ClientSpaceID, WebAuthentication.Instance.CurrentUsername);
			string block = "<div id=\"currentuser-block\">"
						 + "You are currently logged in as <b>{0}</b>."
						 + "</div>";
			admin.AddLeftColumnSection(new RankedString(
				string.Format(block, (user.FirstName + " " + user.Surname).Trim()), -100));

			admin.WebsiteName = CurrentClientSpace.Name;

			if (!SecurityProvider.CurrentUser.HasPermission(PermissionType.UserAdministrator))
				return;

			admin.AddMainMenuLink(new AdminMenuLink("Users and Roles", WebUtility.MakeFullPath("admin/security"), 0));
			
			// build the security interface if it has been requested
			if (SprocketPath.Value.StartsWith("admin/security"))
			{
				handled.Set();

				int defaultMaxFilterMatches;
				try { defaultMaxFilterMatches = int.Parse(SprocketSettings.GetValue("WebSecurityDefaultUserFilterMatches")); }
				catch { defaultMaxFilterMatches = 50; }

				admin.AddInterfaceScript(WebControlScript.TabStrip);
				admin.AddInterfaceScript(WebControlScript.Fader);
				admin.AddInterfaceScript(WebControlScript.AjaxForm);
				string scr = ResourceLoader.LoadTextResource("Sprocket.Security.CMS.security.js")
					.Replace("50,//{defaultMaxFilterMatches}", defaultMaxFilterMatches.ToString() + ",")
					.Replace("if(true)//{ifUserCanAccessRoleManagement}",
						SecurityProvider.CurrentUser.HasPermission(PermissionType.RoleAdministrator) ? "" : "if(false)");
				admin.AddInterfaceScript(new RankedString(scr, 0));
				admin.AddBodyOnLoadScript(new RankedString("SecurityInterface.Run()", 0));
				
				admin.ContentHeading = "Users and Roles";

				string html = "<div id=\"user-admin-container\"></div>";

				admin.AddContentSection(new RankedString(html, 0));
				admin.AddHeadSection(new RankedString("<link rel=\"stylesheet\" type=\"text/css\" href=\""
					+ WebUtility.MakeFullPath("resources/admin/security.css") + "\" />", 0));
			}
		}

		void OnCheckingSettings(SprocketSettings.SettingsErrors errors)
		{
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "WebSecurity"; }
		}

		public string Title
		{
			get { return "Web Security Functions"; }
		}
	}
}
