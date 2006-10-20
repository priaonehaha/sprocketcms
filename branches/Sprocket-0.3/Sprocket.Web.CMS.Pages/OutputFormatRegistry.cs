using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

using Sprocket.Web;

namespace Sprocket.Web.CMS.Pages
{
	public class OutputFormatRegistry
	{
		private XmlDocument outputFormatsDoc = null;
		private Dictionary<string, XmlElement> outputFormats = null;
		private DateTime fileDate = DateTime.MinValue;
		private string outputFormatsDocPath;

		private OutputFormatRegistry()
		{
			Init();
		}

		public static void Reload()
		{
			instance = new OutputFormatRegistry();
		}

		private void Init()
		{
			outputFormatsDocPath = WebUtility.MapPath("resources/definitions/outputformats.xml");
			if (!File.Exists(outputFormatsDocPath)) return;
			outputFormatsDoc = new XmlDocument();
			outputFormatsDoc.Load(outputFormatsDocPath);
			outputFormats = new Dictionary<string, XmlElement>();
			fileDate = File.GetLastWriteTime(outputFormatsDocPath);
		}

		public XmlElement this[string name]
		{
			get
			{
				if (File.GetLastWriteTime(outputFormatsDocPath) > fileDate)
					Init();
				if(outputFormats.ContainsKey(name))
					return outputFormats[name];
				XmlNode node = outputFormatsDoc.SelectSingleNode("/OutputFormats/OutputFormat[@Name='" + name + "']");
				if (node == null)
				{
					outputFormats.Add(name, null);
					return null;
				}
				outputFormats.Add(name, (XmlElement)node);
				return (XmlElement)node;
			}
		}

		#region Instance
		private static OutputFormatRegistry instance = null;
		public static OutputFormatRegistry Formats
		{
			get
			{
				if (instance == null)
					instance = new OutputFormatRegistry();
				return instance;
			}
		}
		#endregion
	}
}
