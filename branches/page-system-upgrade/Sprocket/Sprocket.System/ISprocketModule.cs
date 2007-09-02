using System;
using Sprocket;
using Sprocket.Data;

namespace Sprocket
{
	public interface ISprocketModule
	{
		void AttachEventHandlers(ModuleRegistry registry);
	}
}
