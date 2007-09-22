using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using Sprocket.Utility;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public abstract class Template : IPropertyEvaluatorExpression
	{
		protected string name;
		public string Name
		{
			get { return name; }
		}

		public abstract bool IsOutOfDate { get; }

		#region Expression Members
		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "name":
				case "type":
				case "page_admin_sections":
				case "source":
				case "categorysets":
					return true;
				default: return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "name": return name;
				case "type": return GetType().Name;
				case "page_admin_sections": return pageAdminSections;
				case "categorysets": return categorySets;
				case "source":
					if (this is MasterTemplate)
					{
						if (((MasterTemplate)this).SprocketPath != null)
							return ((MasterTemplate)this).SprocketPath;
						return "HTML in definitions.xml";
					}
					return (this as SubTemplate).MasterTemplateName;

				default: return VariableExpression.InvalidProperty;
			}
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			state.BranchOutput();
			try
			{
				Render(state);
			}
			catch
			{
				state.CancelBranch();
				throw;
			}
			return state.ReadAndRemoveBranch();
		}
		#endregion

		public static Template Create(TemplateRegistry templateRegistry, XmlElement xml)
		{
			if (xml.HasAttribute("Master"))
				return new SubTemplate(templateRegistry, xml);
			return new MasterTemplate(xml);
		}

		internal abstract void Render(ExecutionState state, Dictionary<string, SprocketScript> overrides);

		public void Render(ExecutionState state)
		{
			Render(state, new Dictionary<string, SprocketScript>());
		}

		public string Render(params KeyValuePair<string, object>[] preloadedVariables)
		{
			return RenderIsolated(new ExecutionState(preloadedVariables));
		}

		public string Render()
		{
			return RenderIsolated(new ExecutionState());
		}

		public string RenderIsolated(ExecutionState state)
		{
			state.BranchOutput();
			Render(state);
			return state.ReadAndRemoveBranch();
		}

		protected void ReadPageAdminSettings(XmlElement xml)
		{
			foreach (XmlElement cfxml in xml.SelectNodes("PageAdmin/PageAdminSection"))
				pageAdminSections.Add(ReadPageAdminSectionXml(cfxml));

			foreach (XmlElement cxml in xml.SelectNodes("PageAdmin/CategorySets/CategorySet"))
				categorySets.Add(new CategorySet(cxml));
		}

		private List<CategorySet> categorySets = new List<CategorySet>();
		public List<CategorySet> CategorySets
		{
			get { return categorySets; }
		}

		internal static PageAdminSectionDefinition ReadPageAdminSectionXml(XmlElement cfxml)
		{
			PageAdminSectionDefinition def = new PageAdminSectionDefinition();
			def.SectionName = cfxml.GetAttribute("Name");

			XmlElement e = cfxml.SelectSingleNode("Label") as XmlElement;
			def.Label = e == null ? null : e.InnerText == "" ? null : e.InnerText;

			e = cfxml.SelectSingleNode("Hint") as XmlElement;
			def.Hint = e == null ? null : e.InnerText == "" ? null : e.InnerText;

			if (cfxml.HasAttribute("InSection"))
			{
				string inSection = cfxml.GetAttribute("InSection");
				if (inSection != null && inSection != String.Empty)
					def.InSection = inSection;
			}

			XmlNodeList efnodes = cfxml.SelectNodes("EditFields/*");
			foreach (XmlElement xml in efnodes)
			{
				IEditFieldObjectCreator creator = ContentManager.GetEditFieldObjectCreator(xml.Name);
				if (creator != null)
				{
					IEditFieldHandler handler = creator.CreateHandler();
					handler.Initialise(xml);
					def.EditFieldHandlers.Add(handler);
				}
			}

			return def;
		}

		public List<PageAdminSectionDefinition> GetPageAdminSections()
		{
			List<Template> descendentlist = new List<Template>();
			Template add = this;
			while (true)
			{
				descendentlist.Add(add);
				if (add is MasterTemplate)
					break;
				add = ContentManager.Templates[((SubTemplate)add).MasterTemplateName];
				if (add == null)
					break;
			}

			if (descendentlist[descendentlist.Count - 1] is SubTemplate)
				throw new Exception("Can't load page admin sections. The template hierarchy doesn't end with a master template.");

			// the final list of page admin sections in the correct order:
			List<PageAdminSectionDefinition> list = new List<PageAdminSectionDefinition>();
			// maps page admin sections to the template section that they specify they appear in (if any) so that they can be overridden in subtemplates:
			Dictionary<string, List<PageAdminSectionDefinition>> sectionmap = new Dictionary<string, List<PageAdminSectionDefinition>>();

			// start from the master template and starting building the final page admin section list
			for (int i = descendentlist.Count - 1; i >= 0; i--)
			{
				// if this is a subtemplate, we need to perform surgery on the final list and replace the relevant admin sections as dictated in the xml
				if (descendentlist[i] is SubTemplate)
				{
					// go through each replaced section in the template and extract the page admin sections for that replacement
					foreach (TemplateSectionReplacement rep in ((SubTemplate)descendentlist[i]).ReplacedSections.Values)
					{
						List<PageAdminSectionDefinition> repseclist;
						int index = list.Count; // where to insert the new list of page admin sections
						if (sectionmap.TryGetValue(rep.Name, out repseclist))
							if (repseclist.Count > 0)
							{
								// find where the first replaced page admin section appears in the final list
								index = list.IndexOf(repseclist[0]);
								if (index == -1)
									index = list.Count;

								// remove the to-be-replaced admin sections from the final list
								foreach (PageAdminSectionDefinition sect in repseclist)
									list.Remove(sect);

								// remove the items from the sectionmap now that they're not relevant or needed
								sectionmap.Remove(rep.Name);

								// insert the new page admin sections at the appropriate point in the final list
								list.InsertRange(index, rep.PageAdminSections);

								// go through each section in the new inserts and add them to the map where relevant
								foreach (PageAdminSectionDefinition sect in rep.PageAdminSections)
									if (sect.InSection != null)
									{
										List<PageAdminSectionDefinition> sectlist;
										if (!sectionmap.TryGetValue(sect.InSection, out sectlist))
										{
											sectlist = new List<PageAdminSectionDefinition>();
											sectionmap.Add(sect.InSection, sectlist);
										}
										sectlist.Add(sect);
									}
							}
					}
				}

				// insert the standard page admin sections into the final list
				foreach (PageAdminSectionDefinition section in descendentlist[i].PageAdminSections)
				{
					list.Add(section);

					// if the new section is specified as "InSection", make a record of that
					if (section.InSection != null)
					{
						List<PageAdminSectionDefinition> sectlist;
						if (!sectionmap.TryGetValue(section.InSection, out sectlist))
						{
							sectlist = new List<PageAdminSectionDefinition>();
							sectionmap.Add(section.InSection, sectlist);
						}
						sectlist.Add(section);
					}
				}
			}

			return list;
		}

		private List<PageAdminSectionDefinition> pageAdminSections = new List<PageAdminSectionDefinition>();
		public List<PageAdminSectionDefinition> PageAdminSections
		{
			get { return pageAdminSections; }
		}
	}

	public class MasterTemplate : Template
	{
		SprocketScript script;
		public SprocketScript Script
		{
			get { return script; }
		}

		DateTime fileDate = DateTime.MinValue;
		string filePath = null;
		public string FilePath
		{
			get { return filePath; }
		}

		string sprocketPath = null;
		public string SprocketPath
		{
			get { return sprocketPath; }
		}

		public MasterTemplate(XmlElement xml)
		{
			name = xml.GetAttribute("Name");
			if (xml.HasAttribute("File"))
			{
				sprocketPath = xml.GetAttribute("File");
				filePath = WebUtility.MapPath(sprocketPath);
				if (!File.Exists(filePath))
				{
					script = new SprocketScript("[The template \"" + Name + "\" references a nonexistant file]", "Template: " + Name, "Template: " + Name);
				}
				else
				{
					script = new SprocketScript(File.ReadAllText(filePath), "Template: " + Name, "Template: " + Name);
					fileDate = File.GetLastWriteTime(filePath);
				}
			}
			else
			{
				string text;
				XmlElement body = xml.SelectSingleNode("Body") as XmlElement;
				if (body == null)
					text = "Master template \"" + name + "\" does not have any body text/html. In the XML definition, either reference a file with the \"File=\" attribute, or supply a child element called \"<Body>\" and place the text there.";
				else
					text = body.InnerText;
				script = new SprocketScript(text, "Template: " + name, "Template: " + name);
			}
			ReadPageAdminSettings(xml);
		}

		public override bool IsOutOfDate
		{
			get
			{
				if (script.HasParseError)
					return true;
				if (filePath == null)
					return false;
				else if (!File.Exists(filePath))
					return true;
				return File.GetLastWriteTime(filePath) != fileDate;
			}
		}

		internal override void Render(ExecutionState state, Dictionary<string, SprocketScript> overrides)
		{
			script.SetOverrides(overrides);
			if (!script.Execute(state))
				state.Output.Write(state.ErrorHTML);
			script.RestoreOverrides();
		}
	}

	public class SubTemplate : Template
	{
		private Dictionary<string, TemplateSectionReplacement> replacedSections = new Dictionary<string, TemplateSectionReplacement>();
		private string masterTemplateName = null;
		private TemplateRegistry templateRegistry = null;

		public string MasterTemplateName
		{
			get { return masterTemplateName; }
		}

		public Dictionary<string, TemplateSectionReplacement> ReplacedSections
		{
			get { return replacedSections; }
		}

		public SubTemplate(TemplateRegistry templateRegistry, XmlElement xml)
		{
			this.templateRegistry = templateRegistry;
			name = xml.GetAttribute("Name");
			masterTemplateName = xml.GetAttribute("Master");
			foreach (XmlElement xmlreplace in xml.SelectNodes("Replace[@Section]"))
			{
				TemplateSectionReplacement replacement = new TemplateSectionReplacement(xmlreplace);
				if (replacedSections.ContainsKey(replacement.Name))
					throw new Exception("Error building subpage template. There is more than one replacement defined for section \"" + replacement.Name + "\"");
				replacedSections.Add(replacement.Name, replacement);
			}
			ReadPageAdminSettings(xml);
		}

		public override bool IsOutOfDate
		{
			get
			{
				foreach (TemplateSectionReplacement repl in replacedSections.Values)
					if (repl.IsOutOfDate)
						return true;
				Template t = templateRegistry[masterTemplateName];
				if (t == null)
					return false;
				return t.IsOutOfDate;
			}
		}

		internal override void Render(ExecutionState state, Dictionary<string, SprocketScript> overridesRequestedByChildTemplate)
		{
			Dictionary<string, SprocketScript> overrideRequestsForParentTemplate = new Dictionary<string, SprocketScript>();

			// for each section that the child template wants to override, see if I am already trying to override that section first and
			// absorb the child override into my override script, but use my script as the main script to override that section

			foreach (string sectionName in overridesRequestedByChildTemplate.Keys)
				if (!replacedSections.ContainsKey(sectionName)) // i'm not trying to override that section, so i can pass that override to my parent as though it were mine
					overrideRequestsForParentTemplate.Add(sectionName, overridesRequestedByChildTemplate[sectionName]);

			// put the child override requests into my replacement scripts in case the child requested to replace any of my script sections
			// and then add my replacement requests to the collection be passed to the parent template
			foreach (TemplateSectionReplacement r in replacedSections.Values)
			{
				r.Script.SetOverrides(overridesRequestedByChildTemplate);
				overrideRequestsForParentTemplate.Add(r.Name, r.Script);
			}

			// get the master template and pass the override requests upstairs to be rendered
			Template master = templateRegistry[masterTemplateName];
			if (master == null)
				state.Output.Write("[Template \"" + name + "\" references nonexistant master template \"" + masterTemplateName + "\"]");
			else
				master.Render(state, overrideRequestsForParentTemplate);

			foreach (TemplateSectionReplacement r in replacedSections.Values)
				r.Script.RestoreOverrides(); // clean up
		}
	}

	public class TemplateSectionReplacement
	{
		private string sectionName; //, templateName = null; support templates as replacements some other time
		private SprocketScript script = null;
		private DateTime fileDate = DateTime.MinValue;
		private string filePath = null;
		private Guid guid = Guid.NewGuid();
		private List<PageAdminSectionDefinition> pageAdminSections = new List<PageAdminSectionDefinition>();

		public List<PageAdminSectionDefinition> PageAdminSections
		{
			get { return pageAdminSections; }
		}

		public TemplateSectionReplacement(XmlElement xml)
		{
			sectionName = xml.GetAttribute("Section");
			if (xml.HasAttribute("File"))
			{
				filePath = WebUtility.MapPath(xml.GetAttribute("File"));
				if (!File.Exists(filePath))
					script = new SprocketScript("[Can't replace section \"" + sectionName + "\"; Path \"" + xml.GetAttribute("File") + "\" doesn't exist]",
						"Template Section Replacement: " + sectionName, guid.ToString());
				else
				{
					fileDate = File.GetLastWriteTime(filePath);
					script = new SprocketScript(File.ReadAllText(filePath), "Template Section Replacement: " + sectionName, guid.ToString());
				}
			}
			else
			{
				XmlElement inner = xml.SelectSingleNode("Body") as XmlElement;
				script = new SprocketScript(inner == null ?
					"[Template replacement for section \"" + sectionName + "\" must specify a File attribute or Body child element"
					: inner.InnerText, "Template Section Replacement: " + sectionName, guid.ToString());
			}

			XmlNodeList cfnodes = xml.SelectNodes("PageAdminSections/PageAdminSection");
			foreach (XmlElement cfnode in cfnodes)
				pageAdminSections.Add(Template.ReadPageAdminSectionXml(cfnode));
		}

		public string Name
		{
			get { return sectionName; }
		}

		public SprocketScript Script
		{
			get { return script; }
		}

		public bool IsOutOfDate
		{
			get
			{
				if (script != null)
					if (script.HasParseError)
						return true;
				if (filePath == null)
					return false;
				else if (!File.Exists(filePath))
					return true;
				return fileDate != File.GetLastWriteTime(filePath);
			}
		}
	}
}
