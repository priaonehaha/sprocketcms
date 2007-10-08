using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Transactions;

using Sprocket;
using Sprocket.Data;
using Sprocket.Security;
using Sprocket.Utility;
using Sprocket.Web;
using Sprocket.Web.Controls;
using Sprocket.Web.CMS.Admin;
using Sprocket.Web.CMS.Content;

namespace Sprocket.Web.CMS.Security
{
	[ModuleDependency(typeof(SprocketSettings))]
	[ModuleDependency(typeof(SecurityProvider))]
	[ModuleDependency(typeof(AdminHandler))]
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
					defaultClient = SecurityProvider.DataLayer.SelectClientSpace(SecurityProvider.ClientSpaceID);
				return defaultClient;
			}
		}

		public event RespondableEventHandler<User> OnUserActivated;
		public event RespondableEventHandler<Result> OnUserActivationError;

		public static WebSecurity Instance
		{
			get { return (WebSecurity)Core.Instance[typeof(WebSecurity)].Module; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(OnCheckingSettings);
			AdminHandler.Instance.OnLoadAdminPage += new AdminHandler.AdminRequestHandler(OnAdminRequest);
			//WebsiteAdmin.Instance.OnCMSAdminAuthenticationSuccess += new InterruptableEventHandler<string>(OnCMSAdminAuthenticationSuccess);
			//AdminWindow.Instance.OnBeforeDisplayAdminWindowOverlay += new InterruptableEventHandler(AdminWindow_OnBeforeDisplayAdminWindowOverlay);
			AjaxFormHandler.Instance.OnValidateField += new AjaxFormFieldValidationHandler(OnValidateField);
			AjaxFormHandler.Instance.OnValidateForm += new AjaxFormSubmissionHandler(OnValidateForm);
			AjaxFormHandler.Instance.OnSaveForm += new AjaxFormSubmissionHandler(OnSaveForm);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
			Core.Instance.OnInitialiseComplete += new EmptyHandler(Core_OnInitialiseComplete);

			if(!SecurityProvider.DisableAuthenticationHooks)
				AjaxRequestHandler.Instance.AjaxAuthenticate = AuthenticateForAjaxRequest;
		}

		void Core_OnInitialiseComplete()
		{
			AdminHandler.Instance.GetWebsiteName = GetWebsiteName;
		}

		protected string GetWebsiteName()
		{
			return CurrentClientSpace.Name;
		}

		void WebEvents_OnLoadRequestedPath(HandleFlag handled)
		{
			if (handled.Handled) return;
			switch (SprocketPath.Value)
			{
				case "activate/fix":
					{
						bool failed = false;
						if (!WebAuthentication.IsLoggedIn)
							failed = true;
						else if(!WebAuthentication.VerifyAccess(PermissionType.AdministrativeAccess))
							failed = true;
						if (failed)
						{
							HttpContext.Current.Response.Write("<html><body><p>Access denied. Administrative access required.</p></body></html>");
							handled.Set();
							return;
						}
						else
						{
							try
							{
								int k;
								using (TransactionScope scope = new TransactionScope())
								{
									DatabaseManager.DatabaseEngine.GetConnection();
									List<User> users = SecurityProvider.DataLayer.FilterUsers(null, null, null, null, null, null, false, out k);
									foreach (User user in users)
										SecurityProvider.RequestUserActivation(user.UserID, user.Email);
									scope.Complete();
								}
								HttpContext.Current.Response.Write("<html><body><p>" + k + " activation requests created.</p></body></html>");
								handled.Set();
								return;
							}
							finally
							{
								DatabaseManager.DatabaseEngine.ReleaseConnection();
							}
						}
					}

				default:
					switch (SprocketPath.Sections[0])
					{
						case "_captcha":
							RenderCAPTCHAImage();
							break;

						case "activate":
							if (SprocketPath.Sections.Length == 2)
							{
								string activationCode = SprocketPath.Sections[1];
								long userID;
								Result r = SecurityProvider.DataLayer.ActivateUser(activationCode, out userID);
								if (r.Succeeded)
								{
									User user = null;
									if (WebAuthentication.IsLoggedIn)
										if (SecurityProvider.CurrentUser.UserID == userID)
										{
											user = SecurityProvider.CurrentUser;
											user.Activated = true;
										}
									if (user == null)
										user = SecurityProvider.DataLayer.SelectUser(userID);

									if (OnUserActivated != null)
										OnUserActivated(user, handled);
									if (!handled.Handled)
									{
										HttpContext.Current.Response.Write("<html><body><p>The user has been successfully activated.</p></body></html>");
										handled.Set();
									}
								}
								else
								{
									if (OnUserActivationError != null)
										OnUserActivationError(r, handled);
									if (!handled.Handled)
									{
										HttpContext.Current.Response.Write("<html><body><p>" + r.Message + "</p></body></html>");
										handled.Set();
									}
								}
							}
							break;
					}
					break;
			}
		}

		void AdminWindow_OnBeforeDisplayAdminWindowOverlay(Result result)
		{
			if(WebAuthentication.IsLoggedIn)
				if(WebAuthentication.VerifyAccess(PermissionType.AdministrativeAccess))
					return;
			result.SetFailed("access denied");
		}

		void OnCMSAdminAuthenticationSuccess(string source, Result result)
		{
			if (!WebAuthentication.VerifyAccess(PermissionType.AdministrativeAccess))
				result.SetFailed("You don't have access to this area.");
		}

		void OnAdminRequest(AdminInterface admin, PageEntry page, HandleFlag handled)
		{
			// build the "current user" block
			User user = User.Select(SecurityProvider.ClientSpaceID, WebAuthentication.Instance.CurrentUsername);
			string block = "<div id=\"currentuser-block\">"
						 + "You are currently logged in as <b>{0}</b>."
						 + "</div>";
			admin.AddLeftColumnSection(new AdminSection(
				string.Format(block, (user.FirstName + " " + user.Surname).Trim()), ObjectRank.First));

			if (!WebAuthentication.VerifyAccess(PermissionType.UserAdministrator))
				return;

			admin.AddMainMenuLink(new AdminMenuLink("Users and Roles", WebUtility.MakeFullPath("admin/security"), ObjectRank.Normal));
			
			// build the security interface if it has been requested
			if (SprocketPath.Value.StartsWith("admin/security"))
			{
				//handled.Set();

				int defaultMaxFilterMatches;
				try { defaultMaxFilterMatches = int.Parse(SprocketSettings.GetValue("WebSecurityDefaultUserFilterMatches")); }
				catch { defaultMaxFilterMatches = 50; }

				admin.AddInterfaceScript(WebControlScript.TabStrip);
				admin.AddInterfaceScript(WebControlScript.Fader);
				admin.AddInterfaceScript(WebControlScript.AjaxForm);
				string scr = ResourceLoader.LoadTextResource("Sprocket.Security.CMS.security.js")
					.Replace("50,//{defaultMaxFilterMatches}", defaultMaxFilterMatches.ToString() + ",")
					.Replace("if(true)//{ifUserCanAccessRoleManagement}",
						WebAuthentication.VerifyAccess(PermissionType.RoleAdministrator) ? "" : "if(false)");
				admin.AddInterfaceScript(new AdminSection(scr, 0));
				admin.AddBodyOnLoadScript(new AdminSection("SecurityInterface.Run()", 0));
				
				string html = "<div id=\"user-admin-container\"></div>";

				admin.AddPreContentSection(new AdminSection(html, 0));
				admin.AddHeadSection(new AdminSection("<link rel=\"stylesheet\" type=\"text/css\" href=\""
					+ WebUtility.MakeFullPath("resources/admin/security.css") + "\" />", 0));
			}
		}

		void OnCheckingSettings(SprocketSettings.SettingsErrors errors)
		{
		}
	}
}
