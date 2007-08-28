using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class TemplateRegistry
	{
		Dictionary<string, XmlElement> templateXML = new Dictionary<string, XmlElement>();
		Dictionary<string, Template> templates = new Dictionary<string, Template>();

		public TemplateRegistry() { }

		public TemplateRegistry(XmlElement templatesRootNode)
		{
			Load(templatesRootNode);
		}

		public void Load(XmlElement templatesRootNode)
		{
			foreach (XmlElement xml in templatesRootNode.SelectNodes("Template"))
			{
				if (!xml.HasAttribute("Name"))
					continue;
				templateXML.Add(xml.GetAttribute("Name"), xml);
			}
		}

		public Template this[string id]
		{
			get
			{
				Template t;
				if (templates.TryGetValue(id, out t))
				{
					if (t.IsOutOfDate)
						templates[id] = t = Template.Create(this, templateXML[id]);
				}
				else
					if (templateXML.ContainsKey(id))
						templates[id] = t = Template.Create(this, templateXML[id]);
					else
						return null;
				return t;
			}
		}

		public List<Template> GetList()
		{
			List<Template> list = new List<Template>();
			foreach (string name in templateXML.Keys)
				list.Add(this[name]);
			return list;
		}
	}
}
