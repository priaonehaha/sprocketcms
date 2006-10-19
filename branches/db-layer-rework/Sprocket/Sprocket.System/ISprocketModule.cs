using System;
using Sprocket;

namespace Sprocket
{
	public interface ISprocketModule
	{
		void AttachEventHandlers(ModuleRegistry registry);
		void Initialise(ModuleRegistry registry);
		string RegistrationCode { get; }
		string Title { get; }
		string ShortDescription { get; }
	}

	public delegate void ModuleEnabledStateChangingHandler(ISprocketModule module, StateChangingEventArgs e);

	public interface IOptionalModule
	{
		void DisableModule(bool preserveData);
		void EnableModule();
		event ModuleEnabledStateChangingHandler OnModuleEnabledStateChanging;
		event ModuleEventHandler OnModuleEnabledStateChanged;
	}
}
