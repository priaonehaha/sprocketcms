using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;

namespace Sprocket.Web.CMS.SnapLayouts
{
	public partial class SnapCanvas : IJSONEncoder
	{
		#region Constructor, Fields, Properties, JSON Methods
		#region Fields

		protected long snapCanvasID = 0;
		protected short unitWidth = 0;
		protected short unitHeight = 0;
		protected bool widthExpandable = false;
		protected bool heightExpandable = false;
		protected short unitSize = 0;
		protected short gapSize = 0;

		#endregion

		#region Properties

		///<summary>
		///Gets or sets the value for SnapCanvasID
		///</summary>
		public long SnapCanvasID
		{
			get { return snapCanvasID; }
			set { snapCanvasID = value; }
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
		///Gets or sets the value for WidthExpandable
		///</summary>
		public bool WidthExpandable
		{
			get { return widthExpandable; }
			set { widthExpandable = value; }
		}

		///<summary>
		///Gets or sets the value for HeightExpandable
		///</summary>
		public bool HeightExpandable
		{
			get { return heightExpandable; }
			set { heightExpandable = value; }
		}

		///<summary>
		///Gets or sets the value for UnitSize
		///</summary>
		public short UnitSize
		{
			get { return unitSize; }
			set { unitSize = value; }
		}

		///<summary>
		///Gets or sets the value for GapSize
		///</summary>
		public short GapSize
		{
			get { return gapSize; }
			set { gapSize = value; }
		}

		#endregion

		#region Constructors

		public SnapCanvas()
		{
		}

		public SnapCanvas(long snapCanvasID, short unitWidth, short unitHeight, bool widthExpandable, bool heightExpandable, short unitSize, short gapSize)
		{
			this.snapCanvasID = snapCanvasID;
			this.unitWidth = unitWidth;
			this.unitHeight = unitHeight;
			this.widthExpandable = widthExpandable;
			this.heightExpandable = heightExpandable;
			this.unitSize = unitSize;
			this.gapSize = gapSize;
		}

		public SnapCanvas(IDataReader reader)
		{
			if (reader["SnapCanvasID"] != DBNull.Value) snapCanvasID = (long)reader["SnapCanvasID"];
			if (reader["UnitWidth"] != DBNull.Value) unitWidth = (short)reader["UnitWidth"];
			if (reader["UnitHeight"] != DBNull.Value) unitHeight = (short)reader["UnitHeight"];
			if (reader["WidthExpandable"] != DBNull.Value) widthExpandable = (bool)reader["WidthExpandable"];
			if (reader["HeightExpandable"] != DBNull.Value) heightExpandable = (bool)reader["HeightExpandable"];
			if (reader["UnitSize"] != DBNull.Value) unitSize = (short)reader["UnitSize"];
			if (reader["GapSize"] != DBNull.Value) gapSize = (short)reader["GapSize"];
		}

		#endregion

		#region JSON Methods

		/// <summary>
		/// Writes this entity out as a JSON formatted string
		/// </summary>
		public void WriteJSON(StringWriter writer)
		{
			writer.Write("{");
			JSON.EncodeNameValuePair(writer, "SnapCanvasID", snapCanvasID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitWidth", unitWidth);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitHeight", unitHeight);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "WidthExpandable", widthExpandable);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "HeightExpandable", heightExpandable);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UnitSize", unitSize);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "GapSize", gapSize);
			writer.Write("}");
		}

		#endregion
		#endregion

		private List<SnapPanel> panels = null;
		public List<SnapPanel> Panels
		{
			get
			{
				if (panels == null)
					panels = new List<SnapPanel>();
				return panels;
			}
			set { panels = value; }
		}

		public string RenderHeadBlock(bool editable)
		{
			List<string> headBlocks = new List<string>();
			foreach (SnapPanel panel in panels)
			{
				string head = panel.Widget.HtmlHeadStandardBlock;
				if (!headBlocks.Contains(head))
					headBlocks.Add(head);
				if (editable)
				{
					head = panel.Widget.HtmlHeadEditModeBlock;
					if (!headBlocks.Contains(head))
						headBlocks.Add(head);
				}
			}
			StringBuilder sb = new StringBuilder();
			foreach (string str in headBlocks)
				sb.AppendLine(str);
			return sb.ToString();
		}

		public string RenderInterface(bool editable)
		{
			StringBuilder sb = new StringBuilder(), js = new StringBuilder();
			sb.AppendFormat("<div id=\"canvas-{0}\">", snapCanvasID);
			foreach (SnapPanel panel in Panels)
			{
				sb.Append(panel.Render(unitSize, gapSize));
				if (editable)
				{
					js.Append("canvas");
					js.Append(snapCanvasID);
					js.Append(".Add(");
					js.Append(panel.JavaScriptNewPanelExpression);
					js.AppendLine(");");
				}
			}
			sb.Append("</div>");
			if (editable)
			{
				sb.AppendLine("<script type=\"text/javascript\">");
				sb.Append("var canvas");
				sb.Append(snapCanvasID);
				sb.AppendFormat(" = new Canvas({0}, {1}, {2}, $('canvas-{0}'));", snapCanvasID, unitSize, gapSize);
				sb.Append(Environment.NewLine);
				sb.Append(js);
				sb.AppendLine("</script>");
			}
			return sb.ToString();
		}
	}
}