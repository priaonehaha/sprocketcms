using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web
{
	public static class SprocketPath
	{
		private static string sprocketPath;
		public static string Value
		{
			get { return sprocketPath; }
			internal set { sprocketPath = value; }
		}
	}
}
