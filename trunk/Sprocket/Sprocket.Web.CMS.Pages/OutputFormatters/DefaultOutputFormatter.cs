using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class DefaultOutputFormatter : IOutputFormatter
	{
		public string Format(string input, XmlElement xmlSettings)
		{
			return input;
		}
	}
}
