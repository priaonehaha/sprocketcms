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

		public SnapPanel CreatePanel(SnapCanvas canvas)
		{
			SnapPanel panel = new SnapPanel();
			panel.SnapCanvasID = canvas.SnapCanvasID;
			panel.AllowDelete = true;
			panel.MaxUnitHeight = 0;
			panel.MaxUnitWidth = 0;
			panel.MinUnitHeight = 1;
			panel.MinUnitWidth = 1;
			panel.UnitHeight = 1;
			panel.UnitWidth = 1;
			panel.Widget = this;
			panel.WidgetTypeID = WidgetTypeID;
			panel.LockSize = false;
			panel.LockPosition = false;
			panel.AllowEdit = false;
			return panel;
		}

		private long id;
		public Result LoadSettings(SnapPanel panel)
		{
			id = panel.SnapPanelID;
			return new Result();
		}

		public string Render()
		{
			return "<div style=\"background-color:#afa;width:100%;height:100%;\"></div>";
		}

		public string JavaScriptEditHandlerName
		{
			get { return ""; }
		}

		public string HtmlHeadEditModeBlock
		{
			get { return ""; }
		}

		public string HtmlHeadStandardBlock
		{
			get { return ""; }
		}

		public Result SaveSettings()
		{
			return new Result();
		}

		public Result DeleteSettings()
		{
			return new Result();
		}
	}

	public class ColorWidgetCreator : ISnapPanelWidgetCreator
	{
		public string WidgetTypeID { get { return ColorWidget.WidgetCode; } }

		public ISnapPanelWidget Create()
		{
			return new ColorWidget();
		}

		public void Describe(WidgetDescriptor descriptor)
		{
			descriptor.Title = "Color Widget";
			descriptor.Description = "This is the basic testing widget used for development purposes. It may be later converted into a real widget.";
		}
	}
}
