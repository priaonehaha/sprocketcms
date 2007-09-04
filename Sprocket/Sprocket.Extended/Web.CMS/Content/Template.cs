using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
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
				case "source":
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

		protected void ReadAdminXml(XmlElement xml)
		{
			foreach (XmlElement cfxml in xml.SelectNodes("ContentFields/ContentField"))
			{
				ContentField cf = new ContentField();

				XmlElement e = xml.SelectSingleNode("Label") as XmlElement;
				cf.Label = e == null ? null : e.InnerText == "" ? null : e.InnerText;
				e = xml.SelectSingleNode("DefaultContentType") as XmlElement;
				if (e != null)
				{
					XmlElement def = e.SelectSingleNode("*") as XmlElement;
					if (def != null)
					{
						cf.AllowDeleteDefaultNode = StringUtilities.BoolFromString(e.GetAttribute("AllowDelete"));
						cf.DefaultContentNode = ContentManager.GetNodeType(def.Name);
						cf.DefaultContentNode.Initialise(def);
					}
				}

				e = xml.SelectSingleNode("AllowableNodeTypes") as XmlElement;
				if (e != null)
				{
					if (e.HasAttribute("Maximum"))
					{
						int max;
						if (int.TryParse(e.GetAttribute("Maximum"), out max))
							cf.MaxAdditionalContentNodes = max;
					}

					foreach (XmlElement x in e.SelectNodes("*"))
					{
						IContentNodeType cnt = ContentManager.GetNodeType(x.Name);
						if (cnt != null)
						{
							cnt.Initialise(x);
							cf.AllowableNodeTypes.Add(cnt);
						}
					}
				}
			}
		}

		private List<ContentField> contentFields = new List<ContentField>();
		public List<ContentField> ContentFields
		{
			get { return contentFields; }
		}

		public class ContentField
		{
			private string label = String.Empty;
			private IContentNodeType defaultContentNode = null;
			private bool allowDeleteDefaultNode = false;
			private int? maxAdditionalContentNodes = null;
			private List<IContentNodeType> allowableNodeTypes = new List<IContentNodeType>();

			/// <summary>
			/// If this list is empty, any kind of node can be added to the page.
			/// If MaxAdditionalContentNodes > 0 or is null and this list has one or more items, only items of those types can be added.
			/// If MaxAdditionalContentNodes = 0 OR (AllowableContentNodes is omitted and there is a default node type specified),
			///		new content nodes can't be added to this field on the page.
			/// </summary>
			public List<IContentNodeType> AllowableNodeTypes
			{
				get { return allowableNodeTypes; }
			}

			public int? MaxAdditionalContentNodes
			{
				get { return maxAdditionalContentNodes; }
				internal set { maxAdditionalContentNodes = value; }
			}

			public bool AllowDeleteDefaultNode
			{
				get { return allowDeleteDefaultNode; }
				internal set { allowDeleteDefaultNode = value; }
			}

			public IContentNodeType DefaultContentNode
			{
				get { return defaultContentNode; }
				internal set { defaultContentNode = value; }
			}

			public string Label
			{
				get { return label; }
				internal set { label = value; }
			}
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
			XmlElement adminxml = xml.SelectSingleNode("Admin") as XmlElement;
			if (adminxml != null)
				ReadAdminXml(adminxml);
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
			XmlElement adminxml = xml.SelectSingleNode("Admin") as XmlElement;
			if (adminxml != null)
				ReadAdminXml(adminxml);
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
		Guid guid = Guid.NewGuid();

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
			//else if (xml.HasAttribute("Template"))
			//    templateName = xml.GetAttribute("Template");
			else
				script = new SprocketScript(xml.InnerText, "Template Section Replacement: " + sectionName, guid.ToString());
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
