using System;
using System.Collections.Generic;
using System.Text;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS;
using Sprocket.Web.CMS.Pages;

namespace Sprocket.Web.CMS.SnapLayouts
{
	[ModuleTitle("Snap Layouts")]
	[ModuleDescription("Handles content layout via a click/drag/resize user interface")]
	[AjaxMethodHandler("SnapLayouts")]
	public class SnapLayoutManager : ISprocketModule
	{
		public delegate void PanelLocationUpdateHandler(SnapPanel panel, short unitX, short unitY, short unitWidth, short unitHeight);

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			Core.Instance.OnInitialise += new ModuleInitialisationHandler(Core_OnInitialise);
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(Instance_OnDatabaseHandlerLoaded);
		}

		ISnapLayoutsDataLayer dataLayer = null;
		public static ISnapLayoutsDataLayer DataLayer
		{
			get { return Instance.dataLayer; }
		}

		void Instance_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			source.OnInitialise += new InterruptableEventHandler(DatabaseHandler_OnInitialise);
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(ISnapLayoutsDataLayer)))
			{
				ISnapLayoutsDataLayer layer = (ISnapLayoutsDataLayer)Activator.CreateInstance(t);
				if (layer.DatabaseHandlerType == source.GetType())
				{
					dataLayer = layer;
					break;
				}
			}
		}

		void DatabaseHandler_OnInitialise(Result result)
		{
			if (dataLayer == null)
				result.SetFailed("SnapLayoutHandler has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
			{
				Result r = dataLayer.InitialiseDatabase();
				if (!r.Succeeded)
					result.SetFailed(r.Message);
			}
		}

		void Core_OnInitialise(Dictionary<Type, List<Type>> interfaceImplementations)
		{
			if (interfaceImplementations.ContainsKey(typeof(ISnapPanelWidgetCreator)))
			{
				foreach (Type t in interfaceImplementations[typeof(ISnapPanelWidgetCreator)])
				{
					ISnapPanelWidgetCreator wc = (ISnapPanelWidgetCreator)Activator.CreateInstance(t);
					widgetFactory.Add(wc.WidgetTypeID, wc);
				}
			}
		}

		public static SnapLayoutManager Instance
		{
			get { return (SnapLayoutManager)Core.Instance[typeof(SnapLayoutManager)].Module; }
		}

		public Dictionary<string, ISnapPanelWidgetCreator> widgetFactory = new Dictionary<string, ISnapPanelWidgetCreator>();

		public string RenderCanvas(SnapCanvas canvas, bool editable)
		{
			PrepareCanvas(canvas);
			return canvas.Render(editable);
		}

		public SnapCanvas PrepareCanvas(long canvasID)
		{
			SnapCanvas canvas = dataLayer.SelectSnapCanvas(canvasID);
			PrepareCanvas(canvas);
			return canvas;
		}

		public void PrepareCanvas(SnapCanvas canvas)
		{
			canvas.Panels = dataLayer.ListPanelsForCanvas(canvas.SnapCanvasID);
			foreach (SnapPanel panel in canvas.Panels)
			{
				if (!widgetFactory.ContainsKey(panel.WidgetTypeID)) continue;
				ISnapPanelWidget widget = widgetFactory[panel.WidgetTypeID].Create();
				panel.Widget = widget;
				widget.InitialiseFromPanel(panel);
			}
		}

		public event PanelLocationUpdateHandler OnPanelLocationUpdate;

		[AjaxMethod]
		public void UpdatePanelLocation(long panelID, short unitX, short unitY, short unitWidth, short unitHeight)
		{
			SnapPanel panel = dataLayer.SelectSnapPanel(panelID);
			if(panel == null) return;
			if (OnPanelLocationUpdate != null)
				OnPanelLocationUpdate(panel, unitX, unitY, unitWidth, unitHeight);
		}
	}
}
