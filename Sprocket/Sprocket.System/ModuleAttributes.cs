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
		/// <param name="moduleRegistrationCode">The registration code of the ISprocketModule implementation that this implementation relies upon.</param>
		public ModuleDependencyAttribute(string moduleRegistrationCode)
		{
			regCode = moduleRegistrationCode;
		}

		public string Value
		{
			get { return regCode; }
		}
	}
}
