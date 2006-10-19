using System;

namespace Sprocket
{
	/// <summary>
	/// Specifies that this ISprocketModule implementation contains code that relies
	/// upon the existence of another ISprocketModule implementation. Multiple instances
	/// of this attribute are allowed per class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
	public class ModuleDependencyAttribute : Attribute
	{
		private string regCode;
		/// <summary>
		/// Constructs the attribute.
		/// </summary>
		/// <param name="moduleNamespace">The registration code of the ISprocketModule implementation that this implementation relies upon.</param>
		public ModuleDependencyAttribute(string moduleNamespace)
		{
			regCode = moduleNamespace;
		}

		public string Value
		{
			get { return regCode; }
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public class ModuleDescriptionAttribute : Attribute
	{
		private string description;

		public string Description
		{
			get { return description; }
		}

		public ModuleDescriptionAttribute(string description)
		{
			this.description = description;
		}
	}
}
