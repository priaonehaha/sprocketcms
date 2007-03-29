using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web
{
	public static class SprocketPath
	{
		public static string Value
		{
			get { return (string)CurrentRequest.Value["Sprocket.Web.SprocketPath.Value"]; }
			internal set { CurrentRequest.Value["Sprocket.Web.SprocketPath.Value"] = value; }
		}

		public static string[] Sections
		{
			get { return (string[])CurrentRequest.Value["Sprocket.Web.SprocketPath.PathSections"]; }
			internal set { CurrentRequest.Value["Sprocket.Web.SprocketPath.PathSections"] = value; }
		}
	}
}
