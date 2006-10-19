using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

using Sprocket;
using Sprocket.SystemBase;
using Sprocket.Utility;
using Sprocket.Data;

namespace Sprocket.Web
{
	[ModuleDependency("WebEvents")]
	class SysInfo : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(OnBeginHttpRequest);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
		}

		void OnBeginHttpRequest(HttpApplication app, HandleFlag handled)
		{
			if (handled.Handled)
				return;

			if (app.Context.Request.Path.EndsWith("module-hierarchy-diagram.gif"))
			{
				handled.Set();

				int levels = 0; // the depth of the dependency hierarchy
				int pos = 0; // the number of horizontal positions that this level contains for the bordered boxes
				int maxpos = 1; // the highest box position for the current row
				Dictionary<string, int> modulePositions = new Dictionary<string, int>(); // store which horizontal position each module should have its box drawn in
				Dictionary<int, int> levelCounts = new Dictionary<int, int>(); // specify how many box positions are on each depth level
				foreach (RegisteredModule m in Core.Instance.ModuleRegistry)
				{
					if (m.Importance > levels) // if we've hit the next depth level in the heirarchy
					{
						levels++; // set the number of the level we're now working at
						pos = 1; // specify that we're at horizontal position #1 on the image
					}
					else
					{
						pos++;
						maxpos = maxpos < pos ? pos : maxpos;
					}
					modulePositions[m.Module.RegistrationCode] = pos;
					levelCounts[levels] = pos;
				}

				int rectWidth = 110;
				int rectHeight = 50;
				int heightGap = 25;
				int widthGap = 15;
				int lineGap = 10;
				int bmpWidth = maxpos * rectWidth + (maxpos-1) * widthGap + 11;
				//  bmpHeight = top/bottom margins + combined height of boxes + the gaps between the levels
				int bmpHeight = (heightGap * 2) + (rectHeight * (levels+1)) + (levels * heightGap) + 1;

				Bitmap bmp = new Bitmap(bmpWidth, bmpHeight);
				Graphics gfx = Graphics.FromImage(bmp);
				Pen pen = new Pen(Color.Red, 1);
				Brush whiteBrush = new SolidBrush(Color.White);
				Brush greyBrush = new SolidBrush(Color.WhiteSmoke);
				Brush blackBrush = new SolidBrush(Color.Black);
				Brush redBrush = new SolidBrush(Color.Red);
				Font font = new Font("Verdana", 7, FontStyle.Bold);

				gfx.FillRectangle(whiteBrush, 0, 0, bmpWidth, bmpHeight);
				gfx.SmoothingMode = SmoothingMode.AntiAlias;

				// draw rectangles
				foreach (RegisteredModule m in Core.Instance.ModuleRegistry)
				{
					Rectangle rect = GetModuleRect(m, rectWidth, rectHeight, widthGap, heightGap, modulePositions[m.Module.RegistrationCode], levels, levelCounts[m.Importance], bmpWidth);
					gfx.FillRectangle(greyBrush, rect);
					gfx.DrawRectangle(pen, rect);
				}

				// draw lines
				foreach (RegisteredModule m in Core.Instance.ModuleRegistry)
				{
					Rectangle rect = GetModuleRect(m, rectWidth, rectHeight, widthGap, heightGap, modulePositions[m.Module.RegistrationCode], levels, levelCounts[m.Importance], bmpWidth);

					ModuleDependencyAttribute[] atts = (ModuleDependencyAttribute[])Attribute.GetCustomAttributes(m.GetType(), typeof(ModuleDependencyAttribute), true);
					int attnum = 0;
					foreach (ModuleDependencyAttribute att in atts)
					{
						attnum++;
						RegisteredModule dm = Core.ModuleCore.ModuleRegistry[att.Value];
						int xmodstart = (rectWidth / 2) - ((atts.Length - 1) * lineGap) / 2 + ((attnum - 1) * lineGap);
						int xmodend = Math.Max(bmpWidth / 2 - (levelCounts[dm.Importance] * rectWidth + (levelCounts[dm.Importance] - 1) * widthGap) / 2, 0);
						int level = dm.Importance + 1;
						int dmxpos = modulePositions[dm.Module.RegistrationCode];
						Point start = new Point(rect.X + xmodstart, rect.Y);
						Point end = new Point(xmodend + (dmxpos-1)*rectWidth + (dmxpos-1)*widthGap + rectWidth/2,
							heightGap + level * rectHeight + (level - 1) * heightGap);
						Color color;
						switch (attnum % 7)
						{
							case 0: color = Color.Red; break;
							case 1: color = Color.Green; break;
							case 2: color = Color.Blue; break;
							case 3: color = Color.Violet; break;
							case 4: color = Color.Orange; break;
							case 5: color = Color.DarkCyan; break;
							default: color = Color.DarkSeaGreen; break;
						}
						gfx.DrawLine(new Pen(color), start, end);
						gfx.FillEllipse(new SolidBrush(color), start.X - 2, start.Y - 2, 5, 5);
						gfx.FillEllipse(redBrush, end.X - 2, end.Y - 2, 5, 5);
					}
				}

				// write words
				foreach (RegisteredModule m in Core.Instance.ModuleRegistry)
				{
					Rectangle rect = GetModuleRect(m, rectWidth, rectHeight, widthGap, heightGap, modulePositions[m.Module.RegistrationCode], levels, levelCounts[m.Importance], bmpWidth);
					gfx.DrawString(m.Module.RegistrationCode, font, blackBrush, new PointF(rect.X + 3, rect.Y + 3));
				}

				bmp.Save(app.Context.Response.OutputStream, ImageFormat.Jpeg);
				app.Context.Response.ContentType = "image/jpg";
			}
		}

		private Rectangle GetModuleRect(RegisteredModule m, int rectWidth, int rectHeight, int widthGap, int heightGap, int position, int levels, int modulesOnLevel, int bmpWidth)
		{
			int xmod = Math.Max(bmpWidth / 2 - (modulesOnLevel * rectWidth + (modulesOnLevel - 1) * widthGap) / 2, 0);
			Rectangle rect = new Rectangle();
			rect.Size = new Size(rectWidth, rectHeight);
			rect.X = xmod + (position - 1) * rectWidth + (position - 1) * widthGap;
			rect.Y = heightGap + (m.Importance) * rectHeight + (m.Importance) * heightGap;
			return rect;
		}

		void OnLoadRequestedPath(HttpApplication app, string path, string[] pathSections, HandleFlag handled)
		{
			if (path != "sysinfo")
				return;
			handled.Set();
			string html = ResourceLoader.LoadTextResource("Sprocket.Web.html.sysinfo.htm");
			HttpResponse Response = HttpContext.Current.Response;
			string modules = "<tr>" +
				"<th nowrap=\"true\">Assembly</th>" +
				"<th nowrap=\"true\">Module Code</th>" +
				"<th nowrap=\"true\">Module Name</th>" +
				"<th>Description</th>" +
				"<th>Optional</th>" +
				"<th>DataHandler</th>" +
				"</tr>";
			bool alt = false;
			List<ISprocketModule> bydll = new List<ISprocketModule>();
			foreach (ISprocketModule module in Core.Instance.ModuleRegistry)
				bydll.Add(module);

			bydll.Sort(new ModuleDLLSortComparer());

			string oldf = "";
			bool altf = true;
			bool newdllrow = true;
			foreach (ISprocketModule module in bydll)
			{
				string newf = new FileInfo(module.GetType().Assembly.Location).Name;
				string filename;
				if (oldf != newf)
				{
					filename = newf;
					oldf = newf;
					altf = !altf;
					newdllrow = true;
				}
				else
				{
					filename = "&nbsp;";
					newdllrow = false;
				}
				modules += string.Format(
					"<tr class=\"row-{0}{2}\">" +
					"<td valign=\"top\" class=\"assembly-{1}\">" + filename + "</td>" +
					"<td valign=\"top\" class=\"module-code-{0}\"><strong>" + module.RegistrationCode + "</strong></td>" +
					"<td valign=\"top\" nowrap=\"true\" class=\"module-title-{0}\">" + module.Title + "</td>" +
					"<td valign=\"top\">" + module.ShortDescription + "</td>" +
					"<td valign=\"top\">" + (module is IOptionalModule ? "x" : "&nbsp;") + "</td>" +
					"<td valign=\"top\">" + (module is IDataHandlerModule ? "x" : "&nbsp;") + "</td>" +
					"</tr>",
					alt ? "alt2" : "alt1",
					altf ? "alt2" : "alt1",
					newdllrow ? " newdllrow" : "");
				alt = !alt;
			}
			
			html = html.Replace("{modules}", modules);
			Response.Write(html);
		}

		protected class ModuleDLLSortComparer : IComparer<ISprocketModule>
		{
			public int Compare(ISprocketModule x, ISprocketModule y)
			{
				string ax = new FileInfo(x.GetType().Assembly.Location).Name;
				string ay = new FileInfo(y.GetType().Assembly.Location).Name;
				int z = string.Compare(ax, ay, true);
				if (z != 0) return z;
				return string.Compare(x.RegistrationCode, y.RegistrationCode, true);
			}
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "WebSysInfo"; }
		}

		public string Title
		{
			get { return "Sprocket System Information Viewer"; }
		}

		public string ShortDescription
		{
			get { return "Displays information relating to the current Sprocket installation and setup"; }
		}
	}
}
