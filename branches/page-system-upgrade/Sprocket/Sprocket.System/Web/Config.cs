using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;

namespace Sprocket.Web
{
	[ModuleTitle("Sprocket Configuration Settings")]
	[ModuleDescription("Represents the Sprocket.config settings file")]
	public class Config : ISprocketModule
	{
		private XmlSourceFileDependent configFile = null;
		private string path;

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			path = WebUtility.MapPath("Sprocket.config");
			if (!File.Exists(path))
				File.WriteAllText(path, "<?xml version=\"1.0\"?>" + Environment.NewLine + "<SprocketSettings />");

			configFile = new XmlSourceFileDependent(path);
			configFile.OnFileChanged += new EmptyHandler(configFile_OnFileChanged);
			WebEvents.Instance.OnEndHttpRequest += new WebEvents.HttpApplicationEventHandler(WebEvents_OnEndHttpRequest);
		}

		void WebEvents_OnEndHttpRequest()
		{
			if (xmlChanged)
			{
				XmlDocument.Save(Instance.path);
				fileChanged = true;
			}
			if (fileChanged)
				HttpRuntime.UnloadAppDomain();
		}

		bool fileChanged = false;
		void configFile_OnFileChanged()
		{
			fileChanged = true;
		}

		public static Config Instance
		{
			get { return (Config)Core.Instance[typeof(Config)].Module; }
		}

		bool xmlChanged = false;
		public static void SetSaved()
		{
			Instance.xmlChanged = true;
		}

		public static XmlDocument XmlDocument
		{
			get { return Instance.configFile.Data; }
		}
	}
}
