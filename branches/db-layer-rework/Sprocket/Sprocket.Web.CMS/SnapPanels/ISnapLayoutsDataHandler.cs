using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.SnapLayouts;

namespace Sprocket.Web.CMS
{
	public interface ISnapLayoutsDataLayer
	{
		Type DatabaseHandlerType { get; }
		void InitialiseDatabase(Result result);

		#region Definitions for SnapCanvas

		event InterruptableEventHandler<SnapCanvas> OnBeforeDeleteSnapCanvas;
		event NotificationEventHandler<SnapCanvas> OnSnapCanvasDeleted;
		Result Store(SnapCanvas snapCanvas);
		Result Delete(SnapCanvas snapCanvas);
		SnapCanvas SelectSnapCanvas(long id);

		#endregion

		#region Definitions for SnapPanel

		event InterruptableEventHandler<SnapPanel> OnBeforeDeleteSnapPanel;
		event NotificationEventHandler<SnapPanel> OnSnapPanelDeleted;
		Result Store(SnapPanel snapPanel);
		Result Delete(SnapPanel snapPanel);
		SnapPanel SelectSnapPanel(long id);

		#endregion

		List<SnapPanel> ListPanelsForCanvas(long canvasID);
	}
}
