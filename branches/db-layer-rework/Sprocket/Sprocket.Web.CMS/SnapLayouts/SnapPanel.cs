using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS.SnapLayouts;

namespace Sprocket.Web.CMS.SnapLayouts
{
	public partial class SnapPanel : IJSONEncoder
	{
		#region Constructor, Fields, Properties, JSON Methods
		#region Fields

		protected long snapPanelID = 0;
		protected long snapCanvasID = 0;
		protected string widgetTypeID = "";
		protected short unitWidth = 0;
		protected short unitHeight = 0;
		protected short unitX = 0;
		protected short unitY = 0;
		protected short maxUnitWidth = 0;
		protected short minUnitWidth = 0;
		protected short maxUnitHeight = 0;
		protected short minUnitHeight = 0;
		protected bool lockPosition = false;
		protected bool lockSize = false;
		protected bool allowDelete = false;
		protected bool allowEdit = false;

		#endregion

		#region Properties

		///<summary>
		///Gets or sets the value for SnapPanelID
		///</summary>
		public long SnapPanelID
		{
			get { return snapPanelID; }
			set { snapPanelID = value; }
		}

		///<summary>
		///Gets or sets the value for SnapCanvasID
		///</summary>
		public long SnapCanvasID
		{
			get { return snapCanvasID; }
			set { snapCanvasID = value; }
		}

		///<summary>
		///Gets or sets the value for WidgetTypeID
		///</summary>
		public string WidgetTypeID
		{
			get { return widgetTypeID; }
			set { widgetTypeID = value; }
		}

		///<summary>
		///Gets or sets the value for UnitWidth
		///</summary>
		public short UnitWidth
		{
			get { return unitWidth; }
			set { unitWidth = value; }
		}

		///<summary>
		///Gets or sets the value for UnitHeight
		///</summary>
		public short UnitHeight
		{
			get { return unitHeight; }
			set { unitHeight = value; }
		}

		///<summary>
		///Gets or sets the value for UnitX
		///</summary>
		public short UnitX
		{
			get { return unitX; }
			set { unitX = value; }
		}

		///<summary>
		///Gets or sets the value for UnitY
		///</summary>
		public short UnitY
		{
			get { return unitY; }
			set { unitY = value; }
		}

		///<summary>
		///Gets or sets the value for MaxUnitWidth
		///</summary>
		public short MaxUnitWidth
		{
			get { return maxUnitWidth; }
			set { maxUnitWidth = value; }
		}

		///<summary>
		///Gets or sets the value for MinUnitWidth
		///</summary>
		public short MinUnitWidth
		{
			get { return minUnitWidth; }
			set { minUnitWidth = value; }
		}

		///<summary>
		///Gets or sets the value for MaxUnitHeight
		///</summary>
		public short MaxUnitHeight
		{
			get { return maxUnitHeight; }
			set { maxUnitHeight = value; }
		}

		///<summary>
		///Gets or sets the value for MinUnitHeight
		///</summary>
		public short MinUnitHeight
		{
			get { return minUnitHeight; }
			set { minUnitHeight = value; }
		}

		///<summary>
		///Gets or sets the value for LockPosition
		///</summary>
		public bool LockPosition
		{
			get { return lockPosition; }
			set { lockPosition = value; }
		}

		///<summary>
		///Gets or sets the value for LockSize
		///</summary>
		public bool LockSize
		{
			get { return lockSize; }
			set { lockSize = value; }
		}

		///<summary>
		///Gets or sets the value for AllowDelete
		///</summary>
		public bool AllowDelete
		{
			get { return allowDelete; }
			set { allowDelete = value; }
		}

		///<summary>
		///Gets or sets the value for AllowDelete
		///</summary>
		public bool AllowEdit
		{
			get { return allowEdit; }
			set { allowEdit = value; }
		}

		#endregion

		#region Constructors

		internal SnapPanel()
		{
		}

		internal SnapPanel(long snapPanelID, long snapCanvasID, string widgetTypeID, short unitWidth, short unitHeight, short unitX, short unitY, short maxUnitWidth, short minUnitWidth, short maxUnitHeight, short minUnitHeight, bool lockPosition, bool lockSize, bool allowDelete, bool allowEdit)
		{
			this.snapPanelID = snapPanelID;
			this.snapCanvasID = snapCanvasID;
			this.widgetTypeID = widgetTypeID;
			this.unitWidth = unitWidth;
			this.unitHeight = unitHeight;
			this.unitX = unitX;
			this.unitY = unitY;
			this.maxUnitWidth = maxUnitWidth;
			this.minUnitWidth = minUnitWidth;
			this.maxUnitHeight = maxUnitHeight;
			this.minUnitHeight = minUnitHeight;
			this.lockPosition = lockPosition;
			this.lockSize = lockSize;
			this.allowDelete = allowDelete;
			this.allowEdit = allowEdit;
		}

		internal SnapPanel(IDataReader reader)
		{
			if (reader["SnapPanelID"] != DBNull.Value) snapPanelID = (long)reader["SnapPanelID"];
			if (reader["SnapCanvasID"] != DBNull.Value) snapCanvasID = (long)reader["SnapCanvasID"];
			if (reader["WidgetTypeID"] != DBNull.Value) widgetTypeID = (string)reader["WidgetTypeID"];
			if (reader["UnitWidth"] != DBNull.Value) unitWidth = (short)reader["UnitWidth"];
			if (reader["UnitHeight"] != DBNull.Value) unitHeight = (short)reader["UnitHeight"];
			if (reader["UnitX"] != DBNull.Value) unitX = (short)reader["UnitX"];
			if (reader["UnitY"] != DBNull.Value) unitY = (short)reader["UnitY"];
			if (reader["MaxUnitWidth"] != DBNull.Value) maxUnitWidth = (short)reader["MaxUnitWidth"];
			if (reader["MinUnitWidth"] != DBNull.Value) minUnitWidth = (short)reader["MinUnitWidth"];
			if (reader["MaxUnitHeight"] != DBNull.Value) maxUnitHeight = (short)reader["MaxUnitHeight"];
			if (reader["MinUnitHeight"] != DBNull.Value) minUnitHeight = (short)reader["MinUnitHeight"];
			if (reader["LockPosition"] != DBNull.Value) lockPosition = (bool)reader["LockPosition"];
			if (reader["LockSize"] != DBNull.Value) lockSize = (bool)reader["LockSize"];
			if (reader["AllowDelete"] != DBNull.Value) allowDelete = (bool)reader["AllowDelete"];
			if (reader["AllowEdit"] != DBNull.Value) allowEdit = (bool)reader["AllowEdit"];
		}

		#endregion

		#region JSON Methods

		/// <summary>
		/// Writes this entity out as a JSON formatted string
		/// </summary>
		public void WriteJSON(StringWriter writer)
		{
			writer.Write("{");
			JSON.EncodeNameValuePair(writer, "SnapPanelID", snapPanelID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "SnapCanvasID", snapCanvasID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "WidgetTypeID", widgetTypeID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitWidth", unitWidth);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitHeight", unitHeight);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitX", unitX);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitY", unitY);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "MaxUnitWidth", maxUnitWidth);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "MinUnitWidth", minUnitWidth);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "MaxUnitHeight", maxUnitHeight);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "MinUnitHeight", minUnitHeight);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "LockPosition", lockPosition);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "LockSize", lockSize);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "AllowDelete", allowDelete);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "AllowEdit", allowEdit);
			writer.Write("}");
		}

		#endregion
		#endregion

		public void SetDimensions(short x, short y, short width, short height)
		{
			unitX = x;
			unitY = y;
			unitWidth = width;
			unitHeight = height;
		}

		private ISnapPanelWidget widget;
		public ISnapPanelWidget Widget
		{
			get { return widget; }
			set { widget = value; }
		}

		public string Render(int unitSize, int gapSize)
		{
			string html = widget == null ? "?" : widget.Render();
			int x = unitX * (unitSize + gapSize);
			int y = unitY * (unitSize + gapSize);
			int width = (unitWidth * unitSize) + (unitWidth - 1) * gapSize;
			int height = (unitHeight * unitSize) + (unitHeight - 1) * gapSize;
			return string.Format(
				"<div id=\"panel-{0}\" style=\"position:absolute;width:{1}px;height:{2}px;left:{3}px;top:{4}px;\"><div style=\"width:100%;height:100%;\">{5}</div></div>",
				snapPanelID, width, height, x, y, html);
		}

		public string JavaScriptNewPanelExpression
		{
			get
			{
				string f = widget.JavaScriptEditHandlerName;
				if (f == null)
					f = "null";
				else if (widget.JavaScriptEditHandlerName.Trim() == "")
					f = "null";
				return string.Format(
					"new Panel({4}, {0}, {1}, {2}, {3}, null, $('panel-{4}'), {5}, {6}, {7})",
					UnitX, UnitY, UnitWidth, UnitHeight, snapPanelID,
					allowEdit.ToString().ToLower(), allowDelete.ToString().ToLower(), f
				);
			}
		}
	}
}