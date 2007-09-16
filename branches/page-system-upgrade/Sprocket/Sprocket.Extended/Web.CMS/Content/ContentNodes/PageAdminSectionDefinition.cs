using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sprocket.Utility;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	/// <summary>
	/// Defines the arrangement of a labelled editable area for a page in edit mode.
	/// No page-specific data is held by the class; the fields provided are not for
	/// any purpose other than defining the editable field arrangement for a specific
	/// section in a specific template.
	/// </summary>
	public class PageAdminSectionDefinition : IPropertyEvaluatorExpression
	{
		private string name = String.Empty, label = String.Empty, hint = String.Empty, inSection = null;
		private List<IEditFieldHandler> editFieldHandlers = new List<IEditFieldHandler>();

		public List<IEditFieldHandler> EditFieldHandlers
		{
			get { return editFieldHandlers; }
			internal set { editFieldHandlers = value; }
		}

		public string SectionName
		{
			get { return name; }
			internal set { name = value; }
		}

		public string Label
		{
			get { return label; }
			internal set { label = value; }
		}

		public string Hint
		{
			get { return hint; }
			internal set { hint = value; }
		}

		public string InSection
		{
			get { return inSection; }
			internal set { inSection = value; }
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "label":
				case "hint":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "label":
					return Label;
				case "hint":
					return Hint;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property for this object", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return "PageAdminSection: " + label;
		}

		private string RenderUI()
		{
			return "ui";
		}
	}
}
