using System;
using System.Collections.Generic;
using System.Web;
using Sprocket;
using Sprocket.Web;

namespace Sprocket
{
	public delegate void ModuleInitialisationHandler(Dictionary<Type, List<Type>> interfaceImplementations);

	/// <summary>
	/// The core object which all Sprocket functionality is initiated from. An object of this
	/// type must be created in order for Sprocket to function.
	/// </summary>
	public class Core
	{
		/// <summary>
		/// Called after events have been attached and settings have been validated
		/// </summary>
		public event ModuleInitialisationHandler OnInitialise;
		
		/// <summary>
		/// Called after all modules have been initialised
		/// </summary>
		public event EmptyEventHandler OnInitialiseComplete;

		internal Core()
		{
		}

		private void Initialise()
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
					OnInitialise(moduleHandler.InterfaceImplementations);

				if (OnInitialiseComplete != null)
					OnInitialiseComplete();
			}
		}

		private ModuleHandler moduleHandler;
		private static Core instance = null;
		private static object syncInstance = new object();

		/// <summary>
		/// A reference to the single solitary instance of Core that should exist.
		/// </summary>
		public static Core Instance
		{
			get
			{
				lock (syncInstance)
				{
					if (instance == null)
					{
						instance = new Core();
						instance.Initialise();
					}
				}
				return instance;
			}
		}

		/// <summary>
		/// Resets the Core instance to null, forcing it to reinitialise on the next access.
		/// </summary>
		public static void Reset()
		{
			lock (syncInstance)
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
		public RegisteredModule this[string moduleNamespace]
		{
			get { return moduleHandler[moduleNamespace]; }
		}

		public RegisteredModule this[ISprocketModule module]
		{
			get { return moduleHandler.ModuleRegistry[module.GetType().FullName]; }
		}

		public RegisteredModule this[Type moduleType]
		{
			get { return this[moduleType.FullName]; }
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
