using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

using Sprocket.Web;

namespace Sprocket.Web.CMS.Pages
{
	public class TemplateRegistry
	{
		private XmlDocument templatesDoc = null;
		private Dictionary<string, Template> templates = null;
		private DateTime fileDate = DateTime.MinValue;
		private string templatesDocPath;

		private TemplateRegistry()
		{
			Init();
		}

		public static void Reload()
		{
			instance = new TemplateRegistry();
		}

		private void Init()
		{
			templatesDocPath = WebUtility.MapPath("resources/definitions/templates.xml");
			templatesDoc = new XmlDocument();
			templatesDoc.Load(templatesDocPath);
			templates = new Dictionary<string, Template>();
			fileDate = File.GetLastWriteTime(templatesDocPath);
		}

		public Template this[string name]
		{
			get
			{
				if (File.GetLastWriteTime(templatesDocPath) > fileDate)
					Init();
				return GetTemplate(name);
			}
		}

		private Template GetTemplate(string name)
		{
			return GetTemplate(name, null);
		}

		private Template GetTemplate(string name, Stack<string> embedStack)
		{
			if (templates.ContainsKey(name) && !ContentCache.IsContentCacheDisabled)
				return templates[name];

			Template template;

			if (embedStack == null)
				embedStack = new Stack<string>();
			if (embedStack.Contains(name))
			{
				template = new Template("[Circular dependency detected in template heirarchy at \"" + name + "\"]");
			}
			else
			{
				embedStack.Push(name);

				XmlElement node = (XmlElement)templatesDoc.SelectSingleNode("/Templates/Template[@Name='" + name + "']");
				if (node == null)
				{
					embedStack.Pop();
					template = new Template("[The requested template \"" + name + "\" was not found in template definition file]");
				}
				else
				{
					string text = "";
					if (node.HasAttribute("Master"))
					{
						text = GetTemplate(node.GetAttribute("Master"), embedStack).Text;

						XmlNodeList nodes = node.SelectNodes("Replace");
						foreach (XmlElement repl in nodes)
						{
							if (!repl.HasAttribute("PlaceHolderName"))
								continue;

							string phtext;
							if (repl.HasAttribute("Template"))
								phtext = GetTemplate(repl.GetAttribute("Template"), embedStack).Text;
							else if (repl.HasAttribute("File"))
								phtext = WebUtility.CacheTextFile("resources/templates/" + repl.GetAttribute("File"));
							else if (repl.FirstChild == null)
								phtext = "";
							else if (repl.FirstChild.NodeType == XmlNodeType.Text || repl.FirstChild.NodeType == XmlNodeType.CDATA)
								phtext = repl.FirstChild.Value;
							else
								phtext = "";

							text = Regex.Replace(text, PlaceHolder.GetRegexPattern(repl.GetAttribute("PlaceHolderName")), phtext, RegexOptions.IgnoreCase);
						}
					}
					else if (node.HasAttribute("File"))
					{
						string path = WebUtility.MapPath("resources/templates/" + node.GetAttribute("File"));
						if (!File.Exists(path))
							text = "[Template file \"" + node.GetAttribute("File") + "\" does not exist]";
						else
							text = WebUtility.CacheTextFile("resources/templates/" + node.GetAttribute("File"));
					}
					else
					{
						if (node.FirstChild == null)
							text = "";
						else if (node.FirstChild.NodeType == XmlNodeType.Text || node.FirstChild.NodeType == XmlNodeType.CDATA)
							text = node.FirstChild.Value;
						else
							text = "";
					}
					embedStack.Pop();

					template = new Template(text);
				}
			}
			templates[name] = template;
			return template;
		}

		#region Instance
		private static TemplateRegistry instance = null;
		public static TemplateRegistry Templates
		{
			get
			{
				if (instance == null)
					instance = new TemplateRegistry();
				return instance;
			}
		}
		#endregion
	}
}
