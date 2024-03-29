using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket;
using Sprocket.Utility;
using System.Transactions;

using System.Data;
using Sprocket.Web;

namespace Sprocket.Data
{
	[ModuleDependency(typeof(SprocketSettings))]
	[ModuleDependency(typeof(SystemEvents))]
	[ModuleTitle("Database Manager")]
	[ModuleDescription("Manages configuration and operation of database information relevant to this installation of Sprocket.")]
	public class DatabaseManager : ISprocketModule
	{
		public event NotificationEventHandler<IDatabaseHandler> OnDatabaseHandlerLoaded;

		public static DatabaseManager Instance
		{
			get { return (DatabaseManager)Core.Instance[typeof(DatabaseManager)].Module; }
		}

		public static long GetUniqueID()
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
			{
				return dbHandler.GetUniqueID();
			}
		}

		private static IDatabaseHandler dbHandler = null;
		public static IDatabaseHandler DatabaseEngine
		{
			get
			{
				if (dbHandler == null)
					throw new Exception("OMG! For some reason the dbHandler variable is null!");
				return dbHandler;
			}
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			Core.Instance.OnInitialise += new ModuleInitialisationHandler(Core_OnInitialise);
			SystemEvents.Instance.OnRequestShutDown += new EmptyHandler(Instance_OnRequestShutDown);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(Instance_OnLoadRequestedPath);
		}

		void Instance_OnLoadRequestedPath(HandleFlag handled)
		{
			if(SprocketPath.Sections.Length >= 2)
				if (SprocketPath.Sections[0] == "datastore" && SprocketPath.Sections[1] == "databases")
				{
					HttpContext.Current.Response.Write("access denied.");
					HttpContext.Current.Response.End();
				}
		}

		void Instance_OnRequestShutDown()
		{
			DatabaseEngine.ForceCloseConnection();
		}

		void Core_OnInitialise(Dictionary<Type, List<Type>> interfaceImplementations)
		{
			// need to check web.config to see which database registration name to use
			// instantiate that Type, if found, or throw an error
			// raise a notification event specifying the ISqlDatabase object we're using
			// add an event to this module OnCheckDatabaseStructure, which will eliminate the need for IDataHandlerModule
			if (interfaceImplementations.ContainsKey(typeof(IDatabaseHandler)))
			{
				string databaseEngine = SprocketSettings.GetValue("DatabaseEngine");
				if (databaseEngine == null)
					return;

				foreach (Type t in interfaceImplementations[typeof(IDatabaseHandler)])
					if (t.Name == databaseEngine)
					{
						dbHandler = (IDatabaseHandler)Activator.CreateInstance(t);
						Result result = dbHandler.CheckConfiguration();
						if (!result.Succeeded)
						{
							SprocketSettings.Errors.Add(this, result.Message);
							SprocketSettings.Errors.SetCriticalError();
							return;
						}
						if (OnDatabaseHandlerLoaded != null)
							OnDatabaseHandlerLoaded(dbHandler);
						return;
					}

				List<string> list = new List<string>();
				foreach (Type t in interfaceImplementations[typeof(IDatabaseHandler)])
					list.Add(t.Name);
				SprocketSettings.Errors.Add(this, "The application settings (.config) file requires a valid value for \"DatabaseEngine\".");
				SprocketSettings.Errors.Add(this, "Current valid values for DatabaseEngine are: " + StringUtilities.CommaJoin(list));
				SprocketSettings.Errors.SetCriticalError();
			}
		}
	}
}
