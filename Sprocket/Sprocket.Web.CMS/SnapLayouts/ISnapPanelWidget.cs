using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.SnapLayouts
{
	public interface ISnapPanelWidget
	{
		bool Cacheable { get; }
		SnapPanel CreatePanel(SnapCanvas canvas);
		Result LoadSettings(SnapPanel panel);
		Result SaveSettings();
		Result DeleteSettings();
		string Render();
		string WidgetTypeID { get; }
		string JavaScriptEditHandlerName { get; }
		string HtmlHeadStandardBlock { get; }
		string HtmlHeadEditModeBlock { get; }
	}

	public interface ISnapPanelWidgetCreator
	{
		ISnapPanelWidget Create();
		void Describe(WidgetDescriptor descriptor);
		string WidgetTypeID { get; }
	}
}
