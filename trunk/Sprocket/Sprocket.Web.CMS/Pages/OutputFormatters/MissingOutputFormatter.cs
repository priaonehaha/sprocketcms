using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class MissingOutputFormatter : IOutputFormatter
	{
		public string Format(string input, XmlElement xmlSettings)
		{
			return "[No output formatter exists for the output format type " + xmlSettings.GetAttribute("OutputType") + "]";
		}
	}
}
