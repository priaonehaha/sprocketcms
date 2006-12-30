using System;
using System.Collections.Generic;
using System.Web;

using Sprocket.SystemBase;
using Sprocket.Web;
using Sprocket.Utility;

namespace Sprocket.Web
{
	[ModuleDependency("WebClientScripts")]
	public class WebAuthentication : ISprocketModule
	{
		public delegate void AjaxAuthKeyStoredHandler(string username, Guid authKey);
		public event AjaxAuthKeyStoredHandler OnAjaxAuthKeyStored;

		public static WebAuthentication Instance
		{
			get { return (WebAuthentication)SystemCore.Instance["WebAuthentication"]; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			if(registry.IsRegistered("WebClientScripts"))
				WebClientScripts.Instance.OnBeforeRenderJavaScript += new Sprocket.Web.WebClientScripts.BeforeRenderJavaScriptHandler(OnPreRenderJavaScript);
			AjaxRequestHandler.Instance.OnAjaxRequestAuthenticationCheck += new InterruptableEventHandler<System.Reflection.MethodInfo>(Instance_OnAjaxRequestAuthenticationCheck);
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(OnCheckingSprocketSettings);
		}

		void Instance_OnAjaxRequestAuthenticationCheck(System.Reflection.MethodInfo source, Result result)
		{
			if (!AllowSimultaneousLogins)
				if (AjaxRequestHandler.AuthKey != GetAuthKey(CurrentUsername))
				{
					result.SetFailed("Authentication failed: your account details have been used to log in elsewhere. Please log in again.");
					System.Diagnostics.Trace.WriteLine(CurrentUsername + " / AjaxRequestHandler.AuthKey: " + AjaxRequestHandler.AuthKey + "; GetAuthKey: " + GetAuthKey(CurrentUsername));
				}
		}

		void OnCheckingSprocketSettings(SprocketSettings.SettingsErrors errors)
		{
			string psl = SprocketSettings.GetValue("PreventSimultaneousLogins");
			if (psl == null)
			{
				errors.Add(RegistrationCode, "The Web.config file is missing a value for \"PreventSimultaneousLogins\". The value should be \"True\" or \"False\".");
				errors.SetCriticalError();
				return;
			}
			if (psl.ToLower() != "true" && psl.ToLower() != "false")
			{
				errors.Add(RegistrationCode, "The Web.config file value for \"PreventSimultaneousLogins\" is invalid. The value should be \"True\" or \"False\".");
				errors.SetCriticalError();
				return;
			}
		}

		bool AllowSimultaneousLogins
		{
			get { return !bool.Parse(SprocketSettings.GetValue("PreventSimultaneousLogins")); }
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string Title
		{
			get { return "Web Authentication Manager"; }
		}

		public string ShortDescription
		{
			get { return "Provides an interface for authenticating web requests."; }
		}

		public bool CheckAjaxAuthKey(Guid key)
		{
			return KeyToUser.ContainsKey(key);
		}

		public string GetUsername(Guid key)
		{
			if(!KeyToUser.ContainsKey(key))
				return "";
			return KeyToUser[key].ToString();
		}

		public Guid? GetAuthKey(string username)
		{
			if(!UserToKey.ContainsKey(username))
				return null;
			return UserToKey[username];
		}

		public bool IsValidLogin(string username, string passwordHash)
		{
			if (SystemCore.ModuleCore.SecurityProvider == null) return true; // no security provider = open access
			bool result = SystemCore.ModuleCore.SecurityProvider.IsValidLogin(username, passwordHash);
			return result;
		}

		public void WriteAuthenticationCookie(string username, string passwordHash, Guid ajaxGuid)
		{
			WriteAuthenticationCookie(username, passwordHash, ajaxGuid, 525600);
		}

		public void WriteAuthenticationCookie(string username, string passwordHash, Guid ajaxGuid, int timeoutMinutes)
		{
			HttpCookie cookie = new HttpCookie("Sprocket_Persistent_Login");
			cookie.Values.Add("a", username);
			cookie.Values.Add("b", passwordHash);
			cookie.Values.Add("c", ajaxGuid.ToString());
			cookie.Expires = DateTime.Now.AddMinutes(timeoutMinutes);
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		public void UpdateAuthenticationCookieWithNewAuthKey()
		{
			HttpCookie cookie = HttpContext.Current.Request.Cookies["Sprocket_Persistent_Login"];
			AjaxRequestHandler.AuthKey = StoreAjaxAuthKey(cookie["a"]);
			cookie["c"] = AjaxRequestHandler.AuthKey.ToString();
			cookie.Expires = DateTime.Now.AddYears(1);
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		public void ClearAuthenticationCookie()
		{
			HttpCookie cookie = new HttpCookie("Sprocket_Persistent_Login");
			cookie.Expires = DateTime.Now.AddYears(-1);
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		public bool IsLoggedIn
		{
			get
			{
				HttpCookie cookie = HttpContext.Current.Request.Cookies["Sprocket_Persistent_Login"];
				if (cookie == null)
					return false;
				if (IsValidLogin(cookie["a"], cookie["b"]))
				{

					return true;
				}
				return false;
			}
		}

		public bool ProcessLoginForm(string usernameFieldName, string passwordFieldName, string preserveLoginCheckboxName)
		{
			return ProcessLoginForm(usernameFieldName, passwordFieldName, HttpContext.Current.Request.Form[preserveLoginCheckboxName] != null);
		}

		public bool ProcessLoginForm(string usernameFieldName, string passwordFieldName, bool persistLogin)
		{
			string u = HttpContext.Current.Request.Form[usernameFieldName];
			string p = HttpContext.Current.Request.Form[passwordFieldName];
			int timeout = persistLogin ? 525600 : 60;
			string hash = p == "" ? "" : Crypto.EncryptOneWay(p);
			if (IsValidLogin(u, hash))
			{
				WriteAuthenticationCookie(u, hash, StoreAjaxAuthKey(u), timeout);
				return true;
			}
			ClearAuthenticationCookie();
			return false;
		}

		public void QuickLogin(string username, string password)
		{
			QuickLogin(username, password, false);
		}

		public Guid QuickLogin(string username, string password, bool persistLogin)
		{
			Guid newAuthKey = StoreAjaxAuthKey(username);
			WriteAuthenticationCookie(username, Crypto.EncryptOneWay(password), newAuthKey, persistLogin ? 525600 : 60);
			return newAuthKey;
		}

		private Dictionary<Guid, string> KeyToUser
		{
			get
			{
				if (HttpContext.Current.Application["WebAuthentication_KeyToUser"] == null)
					HttpContext.Current.Application["WebAuthentication_KeyToUser"] = new Dictionary<Guid, string>();
				return (Dictionary<Guid, string>)HttpContext.Current.Application["WebAuthentication_KeyToUser"];
			}
		}

		private Dictionary<string, Guid> UserToKey
		{
			get
			{
				if (HttpContext.Current.Application["WebAuthentication_UserToKey"] == null)
					HttpContext.Current.Application["WebAuthentication_UserToKey"] = new Dictionary<string, Guid>();
				return (Dictionary<string, Guid>)HttpContext.Current.Application["WebAuthentication_UserToKey"];
			}
		}

		public Guid StoreAjaxAuthKey(string username)
		{
			Guid key = Guid.NewGuid(); //new unique key for the user
			KeyToUser.Add(key, username); //add the new key to the list
			if(UserToKey.ContainsKey(username)) //if an existing login for this user exists, remove it
			{
				KeyToUser.Remove(UserToKey[username]); //only one login window at a time, please
				UserToKey.Remove(username); //remove the old username-to-key mapping
			}
			UserToKey.Add(username, key); //add a new username-to-key mapping

			if(OnAjaxAuthKeyStored != null)
				OnAjaxAuthKeyStored(username, key);

			return key;
		}

		public string CurrentUsername
		{
			get
			{
				HttpCookie cookie = HttpContext.Current.Request.Cookies["Sprocket_Persistent_Login"];
				if (cookie == null) return "";
				return cookie["a"];
			}
		}

		private void OnPreRenderJavaScript(JavaScriptCollection scripts)
		{
			HttpContext c = HttpContext.Current;
			HttpCookie authcookie = c.Request.Cookies["Sprocket_Persistent_Login"];
			if (authcookie == null) return;
			scripts.SetKey(AuthKeyPlaceholder, authcookie["c"]);
		}

		public static string AuthKeyPlaceholder
		{
			get { return "$AUTHKEY$"; }
		}

		public string RegistrationCode
		{
			get { return "WebAuthentication"; }
		}
	}
}
