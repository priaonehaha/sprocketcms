using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Transactions;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS;
using Sprocket.Web.CMS.Pages;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.SnapLayouts
{
	[ModuleTitle("Snap Layouts")]
	[ModuleDescription("Handles content layout via a click/drag/resize user interface")]
	[AjaxMethodHandler("SnapLayouts")]
	public class SnapLayoutManager : ISprocketModule
	{
		public delegate void PanelLocationUpdateHandler(SnapPanel panel, short unitX, short unitY, short unitWidth, short unitHeight);
		public delegate void PanelDeleteHandler(SnapPanel panel, Result result);

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
			if (!result.Succeeded)
				return;
			if (dataLayer == null)
				result.SetFailed("SnapLayoutHandler has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
				dataLayer.InitialiseDatabase(result);
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
			string iconPath = WebUtility.MapPath(WidgetDescriptor.DefaultImagePath);
			if (!File.Exists(iconPath))
			{
				Stream stream = ResourceLoader.LoadResource("Sprocket.Web.CMS.SnapLayouts.defaultIcon.gif");
				byte[] bytes = new byte[stream.Length];
				stream.Read(bytes, 0, bytes.Length);
				stream.Close();
				stream.Dispose();
				FileInfo file = new FileInfo(iconPath);
				if (!file.Directory.Exists)
					file.Directory.Create();
				FileStream fs = file.Create();
				fs.Write(bytes, 0, bytes.Length);
				fs.Flush();
				fs.Close();
				fs.Dispose();
			}
		}

		public static SnapLayoutManager Instance
		{
			get { return (SnapLayoutManager)Core.Instance[typeof(SnapLayoutManager)].Module; }
		}

		public Dictionary<string, ISnapPanelWidgetCreator> widgetFactory = new Dictionary<string, ISnapPanelWidgetCreator>();

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
				widget.LoadSettings(panel);
			}
		}

		public static ISnapPanelWidget CreateWidget(string widgetTypeID)
		{
			if (!Instance.widgetFactory.ContainsKey(widgetTypeID))
				return null;
			return Instance.widgetFactory[widgetTypeID].Create();
		}

		public static Point FindBestLocation(SnapCanvas canvas, List<SnapPanel> panels, short targetWidth, short targetHeight)
		{
			short height = canvas.UnitHeight;
			if (height % targetHeight > 0)
				height += (short)(targetHeight - height % targetHeight); // ensures that the height is a perfect multiple of targetHeight
			bool[,] map = new bool[canvas.UnitWidth, height];
			map.Initialize();
			foreach (SnapPanel panel in panels)
				for (short x = panel.UnitX; x < panel.UnitX + panel.UnitWidth; x++)
					for (short y = panel.UnitY; y < panel.UnitY + panel.UnitHeight; y++)
						map[x, y] = true;

			// first quick-find a free point
			for (short y = 0; y < canvas.UnitHeight; y += 1)
				for (short x = 0; x < canvas.UnitWidth - canvas.UnitWidth % targetWidth; x += targetWidth)
					if (!map[x, y])
					{
						// next back-track to the top-left corner of the space
						short xTrackBack = x;
						while (xTrackBack > 0)
							if (map[xTrackBack - 1, y])
								break;
							else
								xTrackBack--;

						// now that we have the corner, check the opposite corner
						if (!map[xTrackBack + targetWidth - 1, y + targetHeight - 1])
						{
							// target corner is empty as well, so do a full scan of the potential panel area
							bool failed = false;
							for (short xSpaceScan = xTrackBack; xSpaceScan < xTrackBack + targetWidth; xSpaceScan++)
							{
								for (short ySpaceScan = y; ySpaceScan < y + targetHeight; ySpaceScan++)
									if (map[xSpaceScan, ySpaceScan])
									{
										failed = true;
										break;
									}
								if (failed)
									break;
							}
							// success! return the point
							if (!failed)
								return new Point(xTrackBack, y);
						}
					}
			//failed to find a spot, so the optimal point is at the start of the first unused row
			return new Point(0, canvas.UnitHeight);
		}

		public event PanelLocationUpdateHandler OnPanelLocationUpdate;
		public event PanelDeleteHandler OnBeforePanelDelete;

		[AjaxMethod(RequiresAuthentication = true)]
		public void UpdatePanelLocation(long panelID, short unitX, short unitY, short unitWidth, short unitHeight)
		{
			SnapPanel panel = dataLayer.SelectSnapPanel(panelID);
			if (panel == null) return;
			if (OnPanelLocationUpdate != null)
				OnPanelLocationUpdate(panel, unitX, unitY, unitWidth, unitHeight);
		}

		[AjaxMethod(RequiresAuthentication = true)]
		public List<WidgetDescriptor> GetWidgetList()
		{
			List<WidgetDescriptor> list = new List<WidgetDescriptor>();
			string defaultPath = WebUtility.MakeFullPath(WidgetDescriptor.DefaultImagePath);
			foreach (KeyValuePair<string, ISnapPanelWidgetCreator> kvp in widgetFactory)
			{
				WidgetDescriptor wd = new WidgetDescriptor(kvp.Key);
				kvp.Value.Describe(wd);
				if (wd.ImageURL == null || wd.ImageURL == "")
					wd.ImageURL = defaultPath;
				list.Add(wd);
			}
			list.Sort(delegate(WidgetDescriptor a, WidgetDescriptor b)
			{
				return a.Title.CompareTo(b.Title);
			});
			return list;
		}

		[AjaxMethod(RequiresAuthentication = true)]
		public Result DeletePanel(long snapPanelID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					DatabaseManager.DatabaseEngine.PersistConnection();

					SnapPanel panel = dataLayer.SelectSnapPanel(snapPanelID);
					if(panel == null)
						return new Result("The panel has already been deleted");

					if (widgetFactory.ContainsKey(panel.WidgetTypeID))
					{
						ISnapPanelWidget widget = widgetFactory[panel.WidgetTypeID].Create();
						Result r = widget.LoadSettings(panel);
						if (!r.Succeeded)
							return r;

						panel.Widget = widget;
					}
					
					Result result = new Result();
					if (OnBeforePanelDelete != null)
						OnBeforePanelDelete(panel, result);
					if (!result.Succeeded)
						return result;

					if (panel.Widget != null)
					{
						result = panel.Widget.DeleteSettings();
						if (!result.Succeeded)
							return result;

						result = dataLayer.Delete(panel);
						if (!result.Succeeded)
							return result;

						scope.Complete();
					}
				}
			}
			catch (Exception ex)
			{
				return new Result(ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}

			return new Result();
		}
	}
}
