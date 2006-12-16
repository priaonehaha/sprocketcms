using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.SnapLayouts
{
	public class ColorWidget : ISnapPanelWidget
	{
		public const string WidgetCode = "ColorWidget";
		public string WidgetTypeID { get { return ColorWidget.WidgetCode; } }

		public bool Cacheable
		{
			get { return true; }
		}

		public SnapPanel CreatePanel()
		{
			SnapPanel panel = new SnapPanel();
			panel.AllowDelete = true;
			panel.MaxUnitHeight = 6;
			panel.MaxUnitWidth = 5;
			panel.MinUnitHeight = 1;
			panel.MinUnitWidth = 1;
			panel.UnitHeight = 1;
			panel.UnitWidth = 1;
			panel.Widget = this;
			panel.WidgetTypeID = WidgetTypeID;
			panel.LockSize = false;
			panel.LockPosition = false;
			panel.AllowDelete = true;
			return panel;
		}

		private long id;
		public Result InitialiseFromPanel(SnapPanel panel)
		{
			id = panel.SnapPanelID;
			return new Result();
		}

		public string Render()
		{
			return "<div style=\"background-color:#afa;width:100%;height:100%;\">" + id + "</div>";
		}

		public string JavaScriptEditHandlerName
		{
			get { return "colorWidgetEdit"; }
		}

		public string HtmlHeadEditModeBlock
		{
			get { return "<script>function colorWidgetEdit(id) { alert(id); }</script>"; }
		}

		public string HtmlHeadStandardBlock
		{
			get { return ""; }
		}
	}

	public class ColorWidgetCreator : ISnapPanelWidgetCreator
	{
		public string WidgetTypeID { get { return ColorWidget.WidgetCode; } }

		public ISnapPanelWidget Create()
		{
			return new ColorWidget();
		}
	}
}
