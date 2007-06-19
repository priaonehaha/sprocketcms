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

		public TemplateRegistry()
		{
			Init();
		}

		public void Init()
		{
			foreach (XmlElement xml in ContentManager.DefinitionsXml.SelectNodes("/Definitions/Templates/Template"))
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
				if(templates.TryGetValue(id, out t))
				{
					if (t.IsOutOfDate)
						templates[id] = t = Template.Create(templateXML[id]);
				}
				else
					templates[id] = t = Template.Create(templateXML[id]);
				return t;
			}
		}

		#region old
		//private Template BuildTemplateScript(string name)
		//{
		//    Dictionary<string, DateTime> fileTimes = new Dictionary<string, DateTime>();
		//    SprocketScript script = BuildTemplateScript(name, fileTimes);
		//    return new Template(name, script, fileTimes);
		//}

		//private SprocketScript BuildTemplateScript(string name, Dictionary<string, DateTime> fileTimes)
		//{
		//    return BuildTemplateScript(name, new Stack<string>(), fileTimes);
		//}

		//private SprocketScript BuildTemplateScript(string name, Stack<string> embedStack, Dictionary<string, DateTime> fileTimes)
		//{
		//    if (embedStack.Contains(name))
		//        return new SprocketScript("[Circular dependency detected in template heirarchy at \"" + name + "\"]", "Template: " + name, "Template: " + name);
		//    embedStack.Push(name);

		//    if (!templateXML.ContainsKey(name))
		//        return new SprocketScript("[There was no template found named \"" + name + "\"]", "Template: " + name, "Template: " + name); ;
		//    XmlElement xml = templateXML[name];

		//    SprocketScript script = null;
		//    if (xml.HasAttribute("Master"))
		//    {
		//        script = BuildTemplateScript(xml.GetAttribute("Master"), embedStack, fileTimes);
		//        foreach (XmlElement replace in xml.SelectNodes("Replace[@Section]"))
		//        {
		//            string sectionName = replace.GetAttribute("Section");
		//            if (replace.HasAttribute("File"))
		//            {
		//                string sprocketPath = replace.GetAttribute("File");
		//                string path = WebUtility.MapPath(sprocketPath);
		//                if (File.Exists(path))
		//                {
		//                    fileTimes[path] = new FileInfo(path).LastWriteTime;
		//                    using (StreamReader reader = new StreamReader(path))
		//                    {
		//                        script.OverrideSection(sectionName, new SprocketScript(reader.ReadToEnd(), sprocketPath, sprocketPath));
		//                        reader.Close();
		//                    }
		//                }
		//                else
		//                    script.OverrideSection(sectionName, new SprocketScript("[Unable to replace section \"" + sectionName + "\". The referenced file doesn't exist]", name, name));
		//            }
		//            else if (replace.HasAttribute("Template"))
		//                script.OverrideSection(sectionName, BuildTemplateScript(replace.GetAttribute("Template"), embedStack, fileTimes));
		//            else
		//            {
		//                if (replace.HasChildNodes)
		//                    script.OverrideSection(sectionName, new SprocketScript(replace.FirstChild.Value, "Template Section " + sectionName, "Template Section " + sectionName));
		//            }
		//        }
		//    }

		//    else if (xml.HasAttribute("File"))
		//    {
		//        //	return new SprocketScript("[The template \"" + name + "\" is lacking a Master or File attribute]", "Template: " + name, "Template: " + name);
		//        string path = WebUtility.MapPath(xml.GetAttribute("File"));
		//        if (!File.Exists(path))
		//            return new SprocketScript("[The template \"" + name + "\" references a nonexistant file]", "Template: " + name, "Template: " + name);
		//        fileTimes[path] = new FileInfo(path).LastWriteTime;
		//        using (StreamReader reader = new StreamReader(path))
		//        {
		//            script = new SprocketScript(reader.ReadToEnd(), "Template: " + name, "Template: " + name);
		//            reader.Close();
		//        }
		//    }
		//    else
		//    {
		//        if (xml.ChildNodes.Count > 0)
		//            if (xml.FirstChild.NodeType == XmlNodeType.CDATA || xml.FirstChild.NodeType == XmlNodeType.Text)
		//                script = new SprocketScript(xml.FirstChild.Value, "Template: " + name, "Template: " + name);
		//        if (script == null)
		//            script = new SprocketScript(xml.InnerText, "Template: " + name, "Template: " + name);
		//    }

		//    embedStack.Pop();
		//    return script;
		//}
		#endregion
	}
}
