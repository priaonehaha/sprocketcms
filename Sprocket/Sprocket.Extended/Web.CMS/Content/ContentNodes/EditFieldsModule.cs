using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Web.FileManager;

namespace Sprocket.Web.CMS.Content.ContentNodes
{
	[ModuleTitle("Edit Fields Module")]
	[ModuleDescription("Provides request handling for internal IEditField implementations")]
	[ModuleDependency(typeof(ContentManager))]
	class EditFieldsModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
		}

		void WebEvents_OnLoadRequestedPath(HandleFlag handled)
		{
			if (handled.Handled) return;
			if (SprocketPath.Sections.Length == 2 && SprocketPath.Sections[0] == "cmsimage" && SprocketPath.Value.EndsWith(".jpg"))
			{
				FileManager.FileManager.Instance.TransmitRequestedImage();
				handled.Set();
			}
		}
	}
}
