using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

using Sprocket.Web;

namespace Sprocket.Web.CMS.Pages
{
	public class GeneralRegistry
	{
		private XmlDocument xmlDoc = null;
		private DateTime fileDate = DateTime.MinValue;
		private string xmlDocPath;

		private GeneralRegistry()
		{
			Init();
		}

		public static void Reload()
		{
			instance = new GeneralRegistry();
		}

		private void Init()
		{
			xmlDocPath = WebUtility.MapPath("resources/definitions/general.xml");
			if (!File.Exists(xmlDocPath))
				return;
			xmlDoc = new XmlDocument();
			xmlDoc.Load(xmlDocPath);
			fileDate = File.GetLastWriteTime(xmlDocPath);
		}

		#region Instance
		private static GeneralRegistry instance = null;
		public static XmlDocument XmlDoc
		{
			get
			{
				if (instance == null)
					instance = new GeneralRegistry();
				return instance.xmlDoc;
			}
		}
		#endregion
	}
}
