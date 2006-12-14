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

		public Result InitialiseFromPanel(SnapPanel panel)
		{
			return new Result();
		}

		public string Render()
		{
			return "<div style=\"background-color:#afa;width:100%;height:100%;\"></div>";
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
