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
using Sprocket.SystemBase;
using Sprocket.Security;
using Sprocket.Utility;
using Sprocket.Web;
using Sprocket.Web.Controls;

namespace Sprocket.Web.CMS.Security
{
	[ModuleDependency("SprocketSettings")]
	[ModuleDependency("SecurityProvider")]
	[ModuleDependency("WebsiteAdmin")]
	public partial class WebSecurity : ISprocketModule
	{
		private static Guid clientID;

		public static Guid WebsiteClientID
		{
			get { return clientID; }
		}

		private static SecurityProvider.Client defaultClient = null;
		public static SecurityProvider.Client WebsiteClient
		{
			get
			{
				if (defaultClient == null)
					defaultClient = new SecurityProvider.Client(WebsiteClientID);
				return defaultClient;
			}
		}

		/// <summary>
		/// Gets a reference to the currently-logged-in user.
		/// </summary>
		public static SecurityProvider.User CurrentUser
		{
			get
			{
				if(CurrentRequest.Value["CurrentUser"] == null)
					CurrentRequest.Value["CurrentUser"] = SecurityProvider.User.Load(WebsiteClientID, ((WebAuthentication)SystemCore.Instance["WebAuthentication"]).CurrentUsername);
				return (SecurityProvider.User)CurrentRequest.Value["CurrentUser"];
			}
			set
			{
				CurrentRequest.Value["CurrentUser"] = value;
			}
		}

		public static WebSecurity Instance
		{
			get { return (WebSecurity)SystemCore.Instance["WebSecurity"]; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(OnCheckingSettings);
			SecurityProvider.Instance.BeforeFirstClientDataInserted += new SecurityProvider.InitialClientHandler(BeforeFirstClientDataInserted);
			WebsiteAdmin.Instance.OnAdminRequest += new WebsiteAdmin.AdminRequestHandler(OnAdminRequest);
			WebsiteAdmin.Instance.OnCMSAdminAuthenticationSuccess += new InterruptableEventHandler<string>(OnCMSAdminAuthenticationSuccess);
			AjaxFormHandler.Instance.OnValidateField += new AjaxFormFieldValidationHandler(OnValidateField);
			AjaxFormHandler.Instance.OnValidateForm += new AjaxFormSubmissionHandler(OnValidateForm);
			AjaxFormHandler.Instance.OnSaveForm += new AjaxFormSubmissionHandler(OnSaveForm);
			AjaxRequestHandler.Instance.OnAjaxRequestAuthenticationCheck += new InterruptableEventHandler<System.Reflection.MethodInfo>(OnAjaxRequestAuthenticationCheck);
		}

		void OnCMSAdminAuthenticationSuccess(string source, Result result)
		{
			if (!CurrentUser.HasPermission("ACCESS_ADMIN"))
				result.SetFailed("You don't have access to this area.");
		}

		void OnAdminRequest(AdminInterface admin, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			// build the "current user" block
			WebAuthentication auth = (WebAuthentication)SystemCore.Instance["WebAuthentication"];
			SecurityProvider.User user = SecurityProvider.User.Load(WebsiteClientID, auth.CurrentUsername);
			string block = "<div id=\"currentuser-block\">"
						 + "You are currently logged in as <b>{0}</b>."
						 + "</div>";
			admin.AddLeftColumnSection(new RankedString(
				string.Format(block, (user.FirstName + " " + user.Surname).Trim()), -100));

			admin.WebsiteName = WebsiteClient.Name;

			if (!CurrentUser.HasPermission(SecurityProvider.PermissionTypeCodes.UserAdministrator))
				return;

			admin.AddMainMenuLink(new AdminMenuLink("Users and Roles", WebUtility.MakeFullPath("admin/security"), 0));
			
			// build the security interface if it has been requested
			if (sprocketPath.StartsWith("admin/security"))
			{
				handled.Set();

				int defaultMaxFilterMatches;
				try { defaultMaxFilterMatches = int.Parse(SprocketSettings.GetValue("WebSecurityDefaultUserFilterMatches")); }
				catch { defaultMaxFilterMatches = 50; }

				admin.AddInterfaceScript(WebControlScript.TabStrip);
				admin.AddInterfaceScript(WebControlScript.Fader);
				admin.AddInterfaceScript(WebControlScript.AjaxForm);
				string scr = ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Security.security.js")
					.Replace("50,//{defaultMaxFilterMatches}", defaultMaxFilterMatches.ToString() + ",")
					.Replace("if(true)//{ifUserCanAccessRoleManagement}",
						CurrentUser.HasPermission("ROLEADMINISTRATOR") ? "" : "if(false)");
				admin.AddInterfaceScript(new RankedString(scr, 0));
				admin.AddBodyOnLoadScript(new RankedString("SecurityInterface.Run()", 0));
				
				admin.ContentHeading = "Users and Roles";
				SecurityProvider security = (SecurityProvider)SystemCore.Instance["SecurityProvider"];

				string html = "<div id=\"user-admin-container\"></div>";

				admin.AddContentSection(new RankedString(html, 0));
				admin.AddHeadSection(new RankedString("<link rel=\"stylesheet\" type=\"text/css\" href=\""
					+ WebUtility.MakeFullPath("resources/admin/security.css") + "\" />", 0));
			}
		}

		void BeforeFirstClientDataInserted(SecurityProvider.InitialClient client)
		{
			client.ClientID = clientID;
		}

		void OnCheckingSettings(SprocketSettings.SettingsErrors errors)
		{
			string path = HttpContext.Current.Request.PhysicalApplicationPath + @"\ClientID.config";
			if (File.Exists(path))
			{
				StreamReader sr = new StreamReader(path);
				string guid = sr.ReadToEnd().Trim();
				try { clientID = new Guid(guid); }
				catch
				{
					errors.Add("WebSecurity", "The existing ClientID.config file contains an invalid unique identifier. If the file has been corrupted, someone with direct database access can retrieve the ClientID from Clients table.");
					errors.SetCriticalError();
					return;
				}
			}
			else
			{
				StreamWriter sw = new StreamWriter(path);
				clientID = Guid.NewGuid();
				sw.Write(clientID.ToString());
				sw.Flush();
				sw.Close();
			}
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

		public string ShortDescription
		{
			get { return "The CMS interface for managing users and roles provided by the SecurityProvider module."; }
		}
	}
}
