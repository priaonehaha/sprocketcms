using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	/// <summary>
	/// Represents a mapping of a page admin section definition to a list of nodes prepared/retrieved
	/// for editing within that section.
	/// </summary>
	public class PreparedPageAdminSection : IPropertyEvaluatorExpression
	{
		private PageAdminSectionDefinition def;
		public PageAdminSectionDefinition SectionDefinition
		{
			get { return def; }
			internal set { def = value; }
		}

		private List<EditFieldInfo> fieldList;
		public List<EditFieldInfo> FieldList
		{
			get { return fieldList; }
			internal set { fieldList = value; }
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "field_list":
				case "section_definition":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "field_list": return fieldList;
				case "section_definition": return def;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property of this object.", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("Can't evaluate this directly. Specify a property of \"field_list\" or \"section_definition\".", contextToken);
		}
	}
}
