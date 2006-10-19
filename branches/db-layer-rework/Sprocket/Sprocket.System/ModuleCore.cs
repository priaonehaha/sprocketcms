using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Web;
using Sprocket.Security;

namespace Sprocket
{
	/// <summary>
	/// Handles discovery, loading, registration and dependency checking of Sprocket modules.
	/// </summary>
	public class ModuleCore
	{
		private ModuleRegistry registry = new ModuleRegistry();
		/// <summary>
		/// Gets a reference to the specified Sprocket module.
		/// </summary>
		/// <param name="moduleRegCode">The registration code for the module to retrieve.</param>
		/// <returns>A reference to the module, or null if the module doesn't exist.</returns>
		public ISprocketModule this[string moduleRegCode]
		{
			get { return registry[moduleRegCode].Module; }
		}

		public ISecurityProvider securityProvider = null;
		/// <summary>
		/// Gets a reference to the default Sprocket module that implements the ISecurityProvider
		/// interface. It is expected that only one such implementation should exist for any
		/// Sprocket-based application. Returns null if no reference exists.
		/// </summary>
		public ISecurityProvider SecurityProvider
		{
			get { return securityProvider; }
		}

		/// <summary>
		/// Searches for assemblies containing Sprocket modules.
		/// </summary>
		public void RegisterModules()
		{
			string dir;
			if(HttpContext.Current == null)
				dir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			else
				dir = HttpContext.Current.Request.PhysicalApplicationPath + "bin";
			RegisterModules(dir);
		}

		/// <summary>
		/// Searches for assemblies containing Sprocket modules.
		/// </summary>
		/// <param name="path">The physical disk location to search for assemblies.</param>
		private void RegisterModules(string path)
		{
			string[] paths = Directory.GetFileSystemEntries(path, "*.dll");
			for(int i=0; i<paths.Length; i++)
			{
				try		{ RegisterFromAssembly(Assembly.LoadFrom(paths[i])); }
				catch	{ continue; }
			}
			foreach(string dir in Directory.GetDirectories(path))
				RegisterModules(dir);
		}

		/// <summary>
		/// Searches an assembly for Sprocket modules and adds them to the module registry.
		/// </summary>
		/// <param name="asm">A reference to the assembly to search</param>
		private void RegisterFromAssembly(Assembly asm)
		{
			foreach(Type t in asm.GetTypes())
			{
				if (t.GetInterface("ISprocketModule") != null)
				{
					ISprocketModule module;
					try
					{
						module = (ISprocketModule)asm.CreateInstance(t.FullName);
						registry.RegisterModule(module);
					}
					catch { continue; }
					if (module is ISecurityProvider)
						securityProvider = (ISecurityProvider)module;
				}
			}
		}

		/// <summary>
		/// Unregisters all modules that are dependent on modules that have not been
		/// loaded into the module registry.
		/// </summary>
		public void CheckDependencies()
		{
			// find all the modules that have a missing dependency...
			List<RegisteredModule> badModules = new List<RegisteredModule>();
			foreach(RegisteredModule module in registry)
			{
				ModuleDependencyAttribute[] atts = (ModuleDependencyAttribute[])Attribute.GetCustomAttributes(module.Module.GetType(), typeof(ModuleDependencyAttribute), true);
				foreach(ModuleDependencyAttribute att in atts)
					if(!registry.IsRegistered(att.Value))
					{
						badModules.Add(module);
						break;
					}
			}

			// ...then unregister them
			foreach (RegisteredModule badmod in badModules)
			{
				if (securityProvider != null)
					if (object.ReferenceEquals(badmod, securityProvider))
						securityProvider = null;
				registry.Unregister(badmod.Module);
			}

			foreach (RegisteredModule module in registry)
				module.Importance = registry.Count;

			// assign an importance value to each remaining module based on dependence heirarchy
			Dictionary<string, RegisteredModule> assignedModules = new Dictionary<string, RegisteredModule>();
			Dictionary<string, RegisteredModule> preAssignedModules = new Dictionary<string, RegisteredModule>();

			int mostImportantSoFar = registry.Count;
			while (assignedModules.Count < registry.Count)
			{
				foreach (RegisteredModule registeredModule in registry)
				{
					bool notyet = false;
					ModuleDependencyAttribute[] atts = (ModuleDependencyAttribute[])Attribute.GetCustomAttributes(registeredModule.GetType(), typeof(ModuleDependencyAttribute), true);
					foreach (ModuleDependencyAttribute att in atts)
						if (!assignedModules.ContainsKey(att.Value))
							notyet = true; // this module doesn't get its importance increased until all of its dependencies are accounted for

					if (notyet)
						continue;

					if (registeredModule.Importance == registry.Count)
						preAssignedModules.Add(registeredModule.Module.RegistrationCode, registeredModule);
					registeredModule.Importance--;
					if (registeredModule.Importance < mostImportantSoFar)
						mostImportantSoFar = registeredModule.Importance;
				}
				foreach (KeyValuePair<string, RegisteredModule> kvp in preAssignedModules)
					assignedModules.Add(kvp.Key, kvp.Value);
				preAssignedModules.Clear();
			}

			// fix the importance values to start from 0 now that we know the depth of the heirarchy
			foreach (RegisteredModule m in registry)
				m.Importance -= mostImportantSoFar;

			// sort the registry so that iterating happens in order of dependence heirarchy depth
			registry.SortByImportance();
		}

		/// <summary>
		/// Tells each module to hook into system events and events of other modules.
		/// </summary>
		public void AttachEventHandlers()
		{
			foreach(RegisteredModule module in registry)
				module.Module.AttachEventHandlers(registry);
		}

		/// <summary>
		/// Tells each module to initialise any internal functionality that does not depend on other modules.
		/// </summary>
		public void InitialiseModules()
		{
			foreach(RegisteredModule module in registry)
				module.Module.Initialise(registry);
		}

		public ModuleRegistry ModuleRegistry
		{
			get { return registry; }
		}
	}

	#region ModuleRegistry

	public class RegisteredModule
	{
		private int importance = 0;
		private ISprocketModule module = null;

		public int Importance
		{
			get { return importance; }
			set { importance = value; }
		}

		public ISprocketModule Module
		{
			get { return module; }
			set { module = value; }
		}

		public RegisteredModule(ISprocketModule module)
		{
			this.module = module;
		}
	}

	/// <summary>
	/// Encapsulates the list of modules that have been registered for use by Sprocket.
	/// </summary>
	public class ModuleRegistry : IEnumerable
	{
		private Dictionary<string, RegisteredModule> moduleRegistry = new Dictionary<string, RegisteredModule>();
		/// <summary>
		/// Adds the specified module instance to the registry.
		/// </summary>
		/// <param name="registrationCode">The registration code for the module</param>
		/// <param name="module">A reference to the module to register</param>
		public void RegisterModule(ISprocketModule module)
		{
			moduleRegistry.Add(module.RegistrationCode, new RegisteredModule(module));
		}

		/// <summary>
		/// Gets a reference to the registered module with the specified registration code.
		/// </summary>
		/// <param name="registrationCode">The registration code for the module</param>
		/// <returns>A reference to a module, or null if it doesn't exist</returns>
		public RegisteredModule this[string registrationCode]
		{
			get
			{
				try { return moduleRegistry[registrationCode]; }
				catch (Exception ex) { throw new SprocketException("The registration code " + registrationCode + " does not exist in the module registry.", ex); }
			}
		}

		/// <summary>
		/// Gets the number of registered modules.
		/// </summary>
		public int Count
		{
			get { return moduleRegistry.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether or not a module with the specified registration code
		/// exists in the module registry.
		/// </summary>
		/// <param name="registrationCode">The registration code to look for</param>
		/// <returns>True if found, otherwise false</returns>
		public bool IsRegistered(string registrationCode)
		{
			return moduleRegistry.ContainsKey(registrationCode);
		}

		/// <summary>
		/// Removes the specified module from the registry. Used for unregistering
		/// modules whose dependencies are missing.
		/// </summary>
		/// <param name="module">A reference to the module to remove.</param>
		public void Unregister(ISprocketModule module)
		{
			if(moduleRegistry.ContainsKey(module.RegistrationCode))
				moduleRegistry.Remove(module.RegistrationCode);
		}

		private bool sorted = false;
		private List<RegisteredModule> sortedRegistry = new List<RegisteredModule>();
		/// <summary>
		/// Sorts all of the modules in the module registry in order of
		/// dependency hierarchy.
		/// </summary>
		public void SortByImportance()
		{
			sortedRegistry.InsertRange(0, moduleRegistry.Values);
			sortedRegistry.Sort(new ModuleImportanceComparer());
			sorted = true;
			
		}

		/// <summary>
		/// Used for comparing the importance values of individual modules.
		/// </summary>
		public class ModuleImportanceComparer : IComparer<RegisteredModule>
		{
			public int Compare(RegisteredModule x, RegisteredModule y)
			{
				if (x.Importance > y.Importance)
					return 1;
				else if (x.Importance < y.Importance)
					return -1;
				else
					return 0;
			}
		}

		/// <summary>
		/// Gets an enumerator for iterating through the module registry.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (sorted)
				return new ModuleRegistryEnumerator(sortedRegistry);
			else
				return new ModuleRegistryEnumerator(moduleRegistry.Values);
		}
	}
	#endregion

	#region ModuleRegistryEnumerator
	public class ModuleRegistryEnumerator : IEnumerator<RegisteredModule>
	{
		IEnumerator<RegisteredModule> regEnum;
		public ModuleRegistryEnumerator(IEnumerable<RegisteredModule> registry)
		{
			regEnum = registry.GetEnumerator();
		}

		public void Reset()
		{
			regEnum.Reset();
		}

		public RegisteredModule Current
		{
			get
			{
				return regEnum.Current;
			}
		}

		public bool MoveNext()
		{
			return regEnum.MoveNext();
		}

		public void Dispose()
		{
		}

		object IEnumerator.Current
		{
			get { return regEnum.Current; }
		}
	}
	#endregion
}
