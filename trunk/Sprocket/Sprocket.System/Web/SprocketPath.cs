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

		public static string Physical
		{
			get
			{
				string s = CurrentRequest.Value["SprocketPath.Physical"] as string;
				if (s == null)
				{
					s = WebUtility.MapPath(Value);
					CurrentRequest.Value["SprocketPath.Physical"] = s;
				}
				return s;
			}
		}
	}
}
