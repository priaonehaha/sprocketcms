using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.SnapLayouts
{
	public interface ISnapPanelWidget
	{
		bool Cacheable { get; }
		SnapPanel CreatePanel();
		Result InitialiseFromPanel(SnapPanel panel);
		string Render();
		string WidgetTypeID { get; }
		string JavaScriptEditHandlerName { get; }
		string HtmlHeadStandardBlock { get; }
		string HtmlHeadEditModeBlock { get; }
	}

	public interface ISnapPanelWidgetCreator
	{
		ISnapPanelWidget Create();
		string WidgetTypeID { get; }
	}
}
