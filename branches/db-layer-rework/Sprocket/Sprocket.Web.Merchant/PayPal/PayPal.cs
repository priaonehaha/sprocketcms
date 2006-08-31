using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket;
using Sprocket.Data;
using Sprocket.SystemBase;
using Sprocket.Utility;

namespace Sprocket.Web.Merchant.PayPal
{
	[ModuleDependency("WebEvents")]
	[ModuleDependency("SprocketSettings")]
	public sealed class PayPal : ISprocketModule, IDataHandlerModule
	{
		public event NotificationEventHandler<PayPalTransactionResponse> OnTransactionResponse;

		void WebEvents_OnLoadRequestedPath(System.Web.HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (!IntegrationEnabled)
				return;

			switch (sprocketPath)
			{
				case "paypal-ipn-process":
					break;

				case "paypal-trans-return":
					TransactionReturn();
					break;

				default:
					return;
			}
			handled.Set();
		}

		void TransactionReturn()
		{
			HttpContext c = HttpContext.Current;
			PaymentDataTransfer pdt = new PaymentDataTransfer();
			pdt.ReadInitialResponse();
			if (!pdt.AuthenticateResponse())
				return;
			pdt.StoreResponseToDatabase();
			if (OnTransactionResponse != null)
				OnTransactionResponse(pdt.TransactionResponse);
		}

		#region Settings Properties
		private static string _ppsetting(string suffix)
		{
			return SprocketSettings.GetValue((TestMode ? "PayPalTest" : "PayPal") + suffix);
		}

		public static bool TestMode
		{
			get { return SprocketSettings.GetBooleanValue("PayPalTestMode"); }
		}

		public static bool IntegrationEnabled
		{
			get { return SprocketSettings.GetBooleanValue("PayPalIntegration"); }
		}

		public static string AccountAddress
		{
			get { return _ppsetting("AccountAddress"); }
		}

		public static string IdentityToken
		{
			get { return _ppsetting("IdentityToken"); }
		}
		#endregion

		#region SprockSettings OnCheckSettings
		void Settings_OnCheckingSettings(SprocketSettings.SettingsErrors errors)
		{
			if (!IntegrationEnabled) 
				return;

			if (TestMode)
			{
				if (SprocketSettings.GetValue("PayPalTestIdentityToken") == null)
				{
					errors.Add("PayPal", "PayPalTestMode setting has been specified, thus a value is required for PayPalTestIdentityToken. This is the PayPal-supplied identity token for use with the PayPal Sandbox development environment. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
				if (SprocketSettings.GetValue("PayPalTestAccountAddress") == null)
				{
					errors.Add("PayPal", "PayPalTestMode setting has been specified, thus a value is required for PayPalTestAccountAddress. This is a test PayPal account address for use with the PayPal Sandbox development environment. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
			}
			else
			{
				if (SprocketSettings.GetValue("PayPalIdentityToken") == null)
				{
					errors.Add("PayPal", "The PayPalTestMode setting is disabled and the PayPalIntegration setting is enabled, thus a value is required for PayPalIdentityToken. This is the PayPal-supplied identity token for authenticating PayPal responses. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
				if (SprocketSettings.GetValue("PayPalAccountAddress") == null)
				{
					errors.Add("PayPal", "The PayPalTestMode setting is disabled and the PayPalIntegration setting is enabled, thus a value is required for PayPalAccountAddress. This is the PayPal account address that is to receive transaction payments. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
			}
		}
		#endregion

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(Settings_OnCheckingSettings);
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "PayPal"; }
		}

		public string Title
		{
			get { return "PayPal Module"; }
		}

		public string ShortDescription
		{
			get { return "Encapsulates PayPal features and merchant facilities"; }
		}

		#endregion

		#region IDataHandlerModule Members

		public void ExecuteDataScripts(DatabaseEngine engine)
		{
			((SqlDatabase)Database.Main).ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Web.Merchant.PayPal.Database.tables.sql"));
			((SqlDatabase)Database.Main).ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Web.Merchant.PayPal.Database.procs.sql"));
		}

		public void DeleteDatabaseStructure(DatabaseEngine engine)
		{
		}

		public bool SupportsDatabaseEngine(DatabaseEngine engine)
		{
			return engine == DatabaseEngine.SqlServer;
		}

		#endregion
	}
}
