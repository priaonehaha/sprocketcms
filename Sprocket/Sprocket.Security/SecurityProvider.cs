using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Transactions;

using Sprocket;
using Sprocket.Data;
using Sprocket.Mail;
using Sprocket.Utility;

namespace Sprocket.Security
{
#warning module dependencies aren't being dealt with properly
	[ModuleDependency(typeof(DatabaseManager))]
	[ModuleDependency(typeof(EmailHandler))]
	[ModuleTitle("Sprocket Security Provider")]
	[ModuleDescription("The default security implementation for Sprocket. Handles users, roles and permissions.")]
	public partial class SecurityProvider : ISprocketModule
	{
		public static SecurityProvider Instance
		{
			get { return (SecurityProvider)Core.Instance[typeof(SecurityProvider)].Module; }
		}

		Dictionary<Type, ISecurityProviderDataLayer> dataLayers = new Dictionary<Type, ISecurityProviderDataLayer>();
		ISecurityProviderDataLayer dataLayer = null;

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(Instance_OnDatabaseHandlerLoaded);
			Core.Instance.OnInitialise += new ModuleInitialisationHandler(Core_OnInitialise);
		}

		void Core_OnInitialise(Dictionary<Type, List<Type>> interfaceImplementations)
		{
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(ISecurityProviderDataLayer)))
			{
				ISecurityProviderDataLayer layer = (ISecurityProviderDataLayer)Activator.CreateInstance(t);
				dataLayers.Add(layer.DatabaseHandlerType, layer);
			}
		}

		void Instance_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			source.OnInitialise += new InterruptableEventHandler(Database_OnInitialise);
			Type t = source.GetType();
			if (dataLayers.ContainsKey(t))
				dataLayer = dataLayers[t];
		}

		void Database_OnInitialise(Result result)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				if (dataLayer == null)
					result.SetFailed("SecurityProvider has no implementation for " + DatabaseManager.DatabaseEngine.Title);
				else
				{
					Result r = dataLayer.InitialiseDatabase(DatabaseManager.DatabaseEngine.CreateDefaultConnection());
					if (!r.Succeeded)
						result.SetFailed(r.Message);
					else
						scope.Complete();
				}
			}
		}

		public static class RoleCodes
		{
			public static string SuperUser { get { return "SUPERUSER"; } }
		}

		public static class PermissionTypeCodes
		{
			public static readonly string UserAdministrator = "USERADMINISTRATOR";
			public static readonly string RoleAdministrator = "ROLEADMINISTRATOR";
		}
	}
}
