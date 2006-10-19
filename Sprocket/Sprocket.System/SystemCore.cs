using System;
using System.Web;
using Sprocket;
using Sprocket.Web;

namespace Sprocket
{
	/// <summary>
	/// The core object which all Sprocket functionality is initiated from. An object of this
	/// type must be created in order for Sprocket to function.
	/// </summary>
	public class Core
	{
		private ModuleCore moduleCore;
		private static Core instance = null;

		public event EmptyEventHandler OnEventsAttached;
		public event EmptyEventHandler OnBeforeSystemInitialised;
		public event EmptyEventHandler OnSystemInitialised;

		/// <summary>
		/// A reference to the single solitary instance of SystemCore that should exist.
		/// </summary>
		public static Core Instance
		{
			get
			{
				if (HttpContext.Current != null) // store it in the application state
				{
					if (HttpContext.Current.Application["Sprocket_SystemCore_Instance"] == null)
						return null;
					return (Core)HttpContext.Current.Application["Sprocket_SystemCore_Instance"];
				}
				else // store it as a static member
				{
					return instance;
				}
			}

			set
			{
				if (HttpContext.Current != null) // store it in the application state
				{
					HttpContext.Current.Application["Sprocket_SystemCore_Instance"] = value;
				}
				else // store it as a static member
				{
					if (instance != null) return;
					instance = value;
				}
			}
		}

		/// <summary>
		/// Gets a reference to the module core, which contains functionality
		/// for retrieving individual modules.
		/// </summary>
		public static ModuleCore ModuleCore
		{
			get { return Instance.moduleCore; }
		}

		/// <summary>
		/// Gets a reference to a specific loaded module, assuming it has been loaded
		/// and registered successfully.
		/// </summary>
		/// <param name="moduleRegistrationCode">The registration code for the module. This is the code specified by the ModuleRegistrationCode attribute.</param>
		/// <returns>A reference to the module, or null if it doesn't exist.</returns>
		public ISprocketModule this[string moduleRegistrationCode]
		{
			get { return moduleCore[moduleRegistrationCode]; }
		}

		/// <summary>
		/// Gets a reference to the module registry, which is a collection of registered modules.
		/// </summary>
		public ModuleRegistry ModuleRegistry
		{
			get { return moduleCore.ModuleRegistry; }
		}

		/// <summary>
		/// Initialises the system core, registers modules, attaches event handlers, etc.
		/// </summary>
		public void Initialise()
		{
			lock (moduleCore)
			{
				if (moduleCore != null)
					return;

				moduleCore = new ModuleCore();
				moduleCore.RegisterModules();
				moduleCore.CheckDependencies();

				moduleCore.AttachEventHandlers();
				if (OnEventsAttached != null)
					OnEventsAttached();

				if (OnBeforeSystemInitialised != null)
					OnBeforeSystemInitialised();

				moduleCore.InitialiseModules();
				if (OnSystemInitialised != null)
					OnSystemInitialised();
			}
		}
	}
}
