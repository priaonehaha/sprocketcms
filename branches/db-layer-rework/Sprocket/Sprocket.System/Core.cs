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
		internal Core()
		{
			lock (moduleHandler)
			{
				if (moduleHandler != null)
					return;

				moduleHandler = new ModuleHandler();
				moduleHandler.RegisterModules();
				moduleHandler.CheckDependencies();
				moduleHandler.AttachEventHandlers();

				SprocketSettings.Instance.ValidateSettings();
				if (!SprocketSettings.Instance.HasErrors)
				{
					if (OnInitialise != null)
						OnInitialise();

					if (OnInitialiseComplete != null)
						OnInitialiseComplete();
				}
			}
		}

		private ModuleHandler moduleHandler;
		private static Core instance = null;

		/// <summary>
		/// Called after events have been attached and settings have been validated
		/// </summary>
		public event EmptyEventHandler OnInitialise;
		
		/// <summary>
		/// Called after all modules have been initialised
		/// </summary>
		public event EmptyEventHandler OnInitialiseComplete;

		/// <summary>
		/// A reference to the single solitary instance of SystemCore that should exist.
		/// </summary>
		public static Core Instance
		{
			get
			{
				lock (instance)
				{
					if (instance == null)
						instance = new Core();
				}
				return instance;
			}
		}

		/// <summary>
		/// Resets the Core instance to null, forcing it to reinitialise on the next access.
		/// </summary>
		public static void Reset()
		{
			lock (instance)
			{
				instance = null;
			}
		}

		/// <summary>
		/// Gets a reference to the module core, which contains functionality
		/// for retrieving individual modules.
		/// </summary>
		public static ModuleHandler Modules
		{
			get { return Instance.moduleHandler; }
		}

		/// <summary>
		/// Gets a reference to a specific loaded module, assuming it has been loaded
		/// and registered successfully.
		/// </summary>
		/// <param name="moduleNamespace">The registration code for the module. This is the code specified by the moduleNamespace attribute.</param>
		/// <returns>A reference to the module, or null if it doesn't exist.</returns>
		public ISprocketModule this[string moduleNamespace]
		{
			get { return moduleHandler[moduleNamespace]; }
		}

		/// <summary>
		/// Gets a reference to the module registry, which is a collection of registered modules.
		/// </summary>
		public ModuleRegistry ModuleRegistry
		{
			get { return moduleHandler.ModuleRegistry; }
		}
	}
}
