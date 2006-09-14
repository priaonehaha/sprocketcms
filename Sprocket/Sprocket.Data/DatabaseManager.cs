using System;
using System.Collections.Generic;
using System.Text;
using Sprocket;
using Sprocket.SystemBase;
using Sprocket.Utility;
using System.Data;

namespace Sprocket.Data
{
	[ModuleDependency("SprocketSettings")]
	[ModuleDependency("SystemEvents")]
	public class DatabaseManager : ISprocketModule
	{
		public static DatabaseManager Instance
		{
			get { return (DatabaseManager)SystemCore.Instance["DatabaseManager"]; }
		}

		public string Title
		{
			get { return "Database Manager"; }
		}

		public string ShortDescription
		{
			get { return "Manages configuration and operation database information relevant to this installation of Sprocket."; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			SprocketSettings.Instance.OnCheckingSettings += new SprocketSettings.CheckSettingsHandler(OnCheckSettings);
			((SprocketSettings)SystemCore.Instance["SprocketSettings"]).OnSettingsVerified += new EmptyEventHandler(OnSettingsVerified);
			((SystemEvents)SystemCore.Instance["SystemEvents"]).OnSessionShutDown += new EmptyEventHandler(OnSessionShutDown);
		}

		void OnSessionShutDown()
		{
			Database.CloseAll();
		}

		void OnSettingsVerified()
		{
			//InitialiseDatabaseStructures(defaultEngine);
		}

		DatabaseEngine defaultEngine;
		public DatabaseEngine DatabaseEngine
		{
			get { return defaultEngine; }
		}

		string defaultConnectionString;
		public string ConnectionString
		{
			get { return defaultConnectionString; }
		}

		void OnCheckSettings(SprocketSettings.SettingsErrors errors)
		{
			if (SprocketSettings.GetValue("ConnectionString") == null)
			{
				errors.Add("DatabaseManager", "The application settings (.config) file requires a valid value for \"ConnectionString\".");
				errors.SetCriticalError();
			}
			if (SprocketSettings.GetValue("DatabaseEngine") == null)
			{
				errors.Add("DatabaseManager", "The application settings (.config) file requires a valid value for \"DatabaseEngine\".");
				errors.SetCriticalError();
			}
			if (errors.HasCriticalError)
				return;

			DatabaseEngine engType;
			try { engType = Database.ParseEngineName(SprocketSettings.GetValue("DatabaseEngine")); }
			catch(SprocketException)
			{
				errors.Add("DatabaseManager", "The value for \"DatabaseEngine\" is not valid.");
				errors.SetCriticalError();
				return;
			}

			Database db = Database.Create(engType);
			db.ConnectionString = SprocketSettings.GetValue("ConnectionString");
			string errorMessage;
			if (!db.TestConnectionString(out errorMessage))
			{
				string msg = errorMessage;
				//if (msg.ToLower().Contains("password")
				//    || msg.ToLower().Contains("pwd")
				//    || msg.ToLower().Contains("pass")
				//    || msg.ToLower().Contains("pword"))
				//    msg = "[error message hidden because it contains password information]";
				errors.Add("DatabaseManager", "The supplied connection string didn't work. The error was: " + msg);
				errors.SetCriticalError();
				return;
			}

			defaultConnectionString = db.ConnectionString;
			defaultEngine = db.DatabaseEngine;
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "DatabaseManager"; }
		}

		public void LoadDefaultDatabase()
		{
			Database db = Database.Create(defaultEngine);
			db.ConnectionString = SprocketSettings.GetValue("ConnectionString");
			Database.Add("DEFAULT", db, true);
		}

		public void ExecuteAllDataScripts(DatabaseEngine engine)
		{
			foreach (RegisteredModule module in SystemCore.Instance.ModuleRegistry)
				if (module.Module is IDataHandlerModule)
					((IDataHandlerModule)module.Module).ExecuteDataScripts(); //engine);
		}
	}
}
