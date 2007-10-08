using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Transactions;

using Sprocket.Web.CMS;
using Sprocket;
using Sprocket.Data;
using Sprocket.Utility;
using Sprocket.Web.Merchant.Database;

namespace Sprocket.Web.Merchant.PayPal
{
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(SprocketSettings))]
	[ModuleTitle("PayPal Module")]
	[ModuleDescription("Encapsulates PayPal features and merchant facilities")]
	public sealed class PayPal : ISprocketModule
	{
		public event NotificationEventHandler<PayPalTransactionResponse> OnTransactionResponse;
		public event NotificationEventHandler<PayPalTransactionResponse> OnInstantPaymentNotification;
		private IMerchantDataProvider dataProvider;

		void WebEvents_OnLoadRequestedPath(HandleFlag handled)
		{
			if (!IntegrationEnabled)
				return;

			switch (SprocketPath.Value)
			{
				case "paypal-ipn-process":
					{
						PayPalTransactionResponse resp = InstantPaymentNotification.Authenticate();
						if (OnInstantPaymentNotification != null && resp != null)
							OnInstantPaymentNotification(resp);
					}

					break;

				case "paypal-trans-return":
					{
						PayPalTransactionResponse resp = TransactionReturn();
						if (OnTransactionResponse != null && resp != null)
							OnTransactionResponse(resp);
					}
					break;

				default:
					return;
			}
			handled.Set();
		}

		PayPalTransactionResponse TransactionReturn()
		{
			HttpContext c = HttpContext.Current;
			PaymentDataTransfer pdt = new PaymentDataTransfer();
			pdt.ReadInitialResponse();
			if (!pdt.AuthenticateResponse())
				return null;
			pdt.StoreResponseToDatabase();
			return pdt.TransactionResponse;
		}

		//void PageRequestHandler_OnRegisteringPlaceHolderRenderers(Dictionary<string, IPlaceHolderRenderer> placeHolderRenderers)
		//{
		//    placeHolderRenderers.Add("paypal", new PayPalPlaceHolderRenderer());
		//}

		#region Settings Properties
		private static string _ppsetting(string suffix)
		{
			return SprocketSettings.GetValue((TestMode ? "PayPalTest" : "PayPal") + suffix);
		}

		public static bool TestMode
		{
			get { return SprocketSettings.GetBooleanValue("PayPalTestMode"); }
		}

		public static string PayPalIpnUrl
		{
			get { return SprocketSettings.GetValue("PayPal-IPN-URL"); }
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

		public static string PayPalPostURL
		{
			get { return "https://www." + (TestMode ? "sandbox." : "") + "paypal.com/cgi-bin/webscr"; }
		}

		public static PayPal Instance
		{
			get { return (PayPal)Core.Instance[typeof(PayPal)].Module; }
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
					errors.Add(this, "PayPalTestMode setting has been specified, thus a value is required for PayPalTestIdentityToken. This is the PayPal-supplied identity token for use with the PayPal Sandbox development environment. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
				if (SprocketSettings.GetValue("PayPalTestAccountAddress") == null)
				{
					errors.Add(this, "PayPalTestMode setting has been specified, thus a value is required for PayPalTestAccountAddress. This is a test PayPal account address for use with the PayPal Sandbox development environment. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
			}
			else
			{
				if (SprocketSettings.GetValue("PayPalIdentityToken") == null)
				{
					errors.Add(this, "The PayPalTestMode setting is disabled and the PayPalIntegration setting is enabled, thus a value is required for PayPalIdentityToken. This is the PayPal-supplied identity token for authenticating PayPal responses. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
				if (SprocketSettings.GetValue("PayPalAccountAddress") == null)
				{
					errors.Add(this, "The PayPalTestMode setting is disabled and the PayPalIntegration setting is enabled, thus a value is required for PayPalAccountAddress. This is the PayPal account address that is to receive transaction payments. See developer.paypal.com for more info.");
					errors.SetCriticalError();
				}
			}
		}
		#endregion

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(DatabaseManager_OnDatabaseHandlerLoaded);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(Settings_OnCheckingSettings);
		}

		void DatabaseManager_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IMerchantDataProvider)))
			{
				IMerchantDataProvider dp = (IMerchantDataProvider)Activator.CreateInstance(t);
				if (dp.DatabaseHandlerType == source.GetType())
				{
					dataProvider = dp;
					break;
				}
			}
			source.OnInitialise += new InterruptableEventHandler(Database_OnInitialise);
		}

		void Database_OnInitialise(Result result)
		{
			if (!result.Succeeded)
				return;
			if (dataProvider == null)
				result.SetFailed("PayPal module has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
			{
				try
				{
					using (TransactionScope scope = new TransactionScope())
					{
						DatabaseManager.DatabaseEngine.GetConnection();
						dataProvider.Initialise(result);
						if (result.Succeeded)
							scope.Complete();
					}
				}
				catch (Exception ex)
				{
					result.SetFailed(ex.Message);
				}
				finally
				{
					DatabaseManager.DatabaseEngine.ReleaseConnection();
				}
			}
		}

		public static IMerchantDataProvider DataProvider
		{
			get
			{
				return (IMerchantDataProvider)Instance.dataProvider;
			}
		}
	}
}
