using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

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
					return true;
				default:
					return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "name":
					return name;
				default:
					return VariableExpression.InvalidProperty;
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

		public static Template Create(XmlElement xml)
		{
			if (xml.HasAttribute("Master"))
				return new SubTemplate(xml);
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

		public MasterTemplate(XmlElement xml)
		{
			name = xml.GetAttribute("Name");
			if (xml.HasAttribute("File"))
			{
				filePath = WebUtility.MapPath(xml.GetAttribute("File"));
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
				script = new SprocketScript(xml.InnerText, "Template: " + name, "Template: " + name);
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
			script.Execute(state);
			script.RestoreOverrides();
		}
	}

	public class SubTemplate : Template
	{
		private Dictionary<string, TemplateSectionReplacement> replacedSections = new Dictionary<string, TemplateSectionReplacement>();
		private string masterTemplateName = null;

		public string MasterTemplateName
		{
			get { return masterTemplateName; }
		}

		public Dictionary<string, TemplateSectionReplacement> ReplacedSections
		{
			get { return replacedSections; }
		}

		public SubTemplate(XmlElement xml)
		{
			name = xml.GetAttribute("Name");
			masterTemplateName = xml.GetAttribute("Master");
			foreach (XmlElement xmlreplace in xml.SelectNodes("Replace[@Section]"))
			{
				TemplateSectionReplacement replacement = new TemplateSectionReplacement(xmlreplace);
				if (replacedSections.ContainsKey(replacement.Name))
					throw new Exception("Error building subpage template. There is more than one replacement defined for section \"" + replacement.Name + "\"");
				replacedSections.Add(replacement.Name, replacement);
			}
		}

		public override bool IsOutOfDate
		{
			get
			{
				foreach (TemplateSectionReplacement repl in replacedSections.Values)
					if (repl.IsOutOfDate)
						return true;
				Template t = ContentManager.Templates[masterTemplateName];
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
			Template master = ContentManager.Templates[masterTemplateName];
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
