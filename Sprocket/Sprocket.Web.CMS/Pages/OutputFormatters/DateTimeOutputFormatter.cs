using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class DateTimeOutputFormatter : IOutputFormatter
	{
		public string Format(string input, System.Xml.XmlElement xmlSettings)
		{
			DateTime dt;
			bool s = DateTime.TryParse(input, out dt);
			if (!s) return "[Bad DateTime Format: " + input + "]";
			XmlNode node = xmlSettings.SelectSingleNode("./FormatString");
			if (node.FirstChild == null)
				return dt.ToString();
			if (node.FirstChild.NodeType != XmlNodeType.CDATA && node.FirstChild.NodeType != XmlNodeType.Text)
				return dt.ToString();
			try
			{
				return dt.ToString(node.FirstChild.Value);
			}
			catch
			{
				return "[Invalid DateTime Format: " + node.FirstChild.Value + "]";
			}
		}
	}
}
