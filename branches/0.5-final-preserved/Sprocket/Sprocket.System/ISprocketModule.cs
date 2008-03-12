using System;
using Sprocket;

namespace Sprocket
{
	public interface ISprocketModule
	{
		void AttachEventHandlers(ModuleRegistry registry);
	}
}
