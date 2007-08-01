using System;
using System.Collections;
using System.Web;

using Sprocket;
using Sprocket.Web;
using Sprocket.Utility;

namespace Sprocket.Web
{
	public delegate Result LoginAuthenticationHandler(string username, string passwordHash);

	[ModuleDependency(typeof(WebClientScripts))]
	[ModuleDescription("Provides an interface for authenticating web requests.")]
	[ModuleTitle("Web Authentication Manager")]
	public class WebAuthentication : ISprocketModule
	{
		private const string cookieKey = "Sprocket_Auth_Key";
		public delegate void AjaxAuthKeyStoredHandler(string username, Guid authKey);
		public event AjaxAuthKeyStoredHandler OnAjaxAuthKeyStored;
		public LoginAuthenticationHandler Authenticate = null;

		private Hashtable usersByKey = new Hashtable();
		private Hashtable keysByUser = new Hashtable();

		public static WebAuthentication Instance
		{
			get { return (WebAuthentication)Core.Instance[typeof(WebAuthentication)].Module; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			if(registry.IsRegistered("WebClientScripts"))
				WebClientScripts.Instance.OnBeforeRenderJavaScript += new Sprocket.Web.WebClientScripts.BeforeRenderJavaScriptHandler(OnPreRenderJavaScript);
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(OnCheckingSprocketSettings);
		}

		void OnCheckingSprocketSettings(SprocketSettings.SettingsErrors errors)
		{
			string psl = SprocketSettings.GetValue("PreventSimultaneousLogins");
			if (psl == null)
			{
				errors.Add(this, "The Web.config file is missing a value for \"PreventSimultaneousLogins\". The value should be \"True\" or \"False\".");
				errors.SetCriticalError();
				return;
			}
			if (psl.ToLower() != "true" && psl.ToLower() != "false")
			{
				errors.Add(this, "The Web.config file value for \"PreventSimultaneousLogins\" is invalid. The value should be \"True\" or \"False\".");
				errors.SetCriticalError();
				return;
			}
		}

		bool AllowSimultaneousLogins
		{
			get { return !bool.Parse(SprocketSettings.GetValue("PreventSimultaneousLogins")); }
		}

		public bool CheckAjaxAuthKey(Guid key)
		{
			return usersByKey.ContainsKey(key);
		}

		public string GetUsername(Guid key)
		{
			if(!usersByKey.ContainsKey(key))
				return "";
			return usersByKey[key].ToString();
		}

		public string GetAuthKey(string username)
		{
			if(!keysByUser.ContainsKey(username))
				return "";
			return keysByUser[username].ToString();
		}

		public Result ValidateLogin(string username, string passwordHash)
		{
			Result result = new Result();
			if (Authenticate != null)
				return Authenticate(username, passwordHash);
			return result;
		}

		public void WriteAuthenticationCookie(string username, string passwordHash, Guid ajaxGuid)
		{
			WriteAuthenticationCookie(username, passwordHash, ajaxGuid, 525600);
		}

		public void WriteAuthenticationCookie(string username, string passwordHash, Guid ajaxGuid, int timeoutMinutes)
		{
			string key = PassKeyFromPasswordHash(passwordHash);
			HttpCookie cookie = new HttpCookie(cookieKey);
			cookie.Values.Add("a", username);
			cookie.Values.Add("k", key);
			cookie.Values.Add("c", Guid.NewGuid().ToString());
			cookie.Expires = DateTime.Now.AddMinutes(timeoutMinutes);
			#warning how is this affected by UTC?
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		private string PasswordHashFromPassKey(string passKey)
		{
			string startIP = HttpContext.Current.Request.UserHostAddress;
			startIP = startIP.Substring(0, startIP.LastIndexOf('.'));
			string encKey = SprocketSettings.GetValue("EncryptionKeyWord");
			if (encKey == null)
				throw new Exception("Please add a kay named \"EncryptionKeyWord\" to your Web.Config file. This is a secret keyword or phrase of your choice.");
			return Crypto.RC2Decrypt(StringUtilities.BytesFromHexString(passKey), encKey, startIP);
		}

		private string PassKeyFromPasswordHash(string passwordHash)
		{
			string startIP = HttpContext.Current.Request.UserHostAddress;
			startIP = startIP.Substring(0, startIP.LastIndexOf('.'));
			string encKey = SprocketSettings.GetValue("EncryptionKeyWord");
			return StringUtilities.HexStringFromBytes(Crypto.RC2Encrypt(passwordHash, encKey, startIP));
		}

		public void ClearAuthenticationCookie()
		{
			HttpCookie cookie = new HttpCookie(cookieKey);
			cookie.Expires = DateTime.Now.AddYears(-1);
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		public static bool IsLoggedIn
		{
			get
			{
				if (CurrentRequest.Value["CurrentUser_Authenticated"] == null)
				{
					HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieKey];
					if (cookie == null)
						return false;
					if (cookie.Value == "")
						return false;

					string passkey;
					bool result;
					try
					{
						passkey = Instance.PasswordHashFromPassKey(cookie["k"]);
						result = Instance.ValidateLogin(cookie["a"], passkey).Succeeded;
					}
					catch
					{
						result = false;
					}

					CurrentRequest.Value["CurrentUser_Authenticated"] = result;
					return result;
				}
				else
					return (bool)CurrentRequest.Value["CurrentUser_Authenticated"];
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
			if (ValidateLogin(u, hash).Succeeded)
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

		public void QuickLogin(string username, string password, bool persistLogin)
		{
			WriteAuthenticationCookie(username, Crypto.EncryptOneWay(password), StoreAjaxAuthKey(username), persistLogin ? 525600 : 60);
			CurrentRequest.Value["CurrentUser_Authenticated"] = true;
		}

		public Guid StoreAjaxAuthKey(string username)
		{
			Guid key = Guid.NewGuid(); //new unique key for the user
			usersByKey.Add(key, username); //add the new key to the list
			if(keysByUser.ContainsKey(username)) //if an existing login for this user exists, remove it
			{
				usersByKey.Remove(keysByUser[username]); //only one login window at a time, please
				keysByUser.Remove(username); //remove the old username-to-key mapping
			}
			keysByUser.Add(username, key); //add a new username-to-key mapping

			if(OnAjaxAuthKeyStored != null)
				OnAjaxAuthKeyStored(username, key);

			return key;
		}

		public string CurrentUsername
		{
			get
			{
				HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieKey];
				if(cookie == null) return "";
				return cookie["a"];
			}
		}

		private void OnPreRenderJavaScript(JavaScriptCollection scripts)
		{
			HttpContext c = HttpContext.Current;
			HttpCookie authcookie = c.Request.Cookies[cookieKey];
			if (authcookie == null) return;
			scripts.SetKey(AuthKeyPlaceholder, authcookie["c"]);
		}

		public static string AuthKeyPlaceholder
		{
			get { return "$AUTHKEY$"; }
		}
	}
}
