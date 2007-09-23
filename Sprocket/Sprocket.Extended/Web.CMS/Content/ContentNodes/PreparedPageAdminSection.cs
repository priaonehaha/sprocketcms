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
	public class PreparedPageAdminSection : IPropertyEvaluatorExpression, IArgumentListEvaluatorExpression
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
			get
			{
				return fieldList;
			}
			internal set
			{
				fieldList = value;
			}
		}

		private Dictionary<string, EditFieldInfo> fieldsByName;
		public Dictionary<string, EditFieldInfo> FieldsByName
		{
			get { return fieldsByName; }
		}

		public void UpdateFieldsByName()
		{
			fieldsByName = new Dictionary<string, EditFieldInfo>();
			foreach (EditFieldInfo info in fieldList)
				fieldsByName[info.FieldName] = info;
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "field_list":
				case "section_definition":
				case "fields_by_name":
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
				case "fields_by_name": return fieldsByName;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property of this object.", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("Can't evaluate this directly. Specify a property of \"field_list\" or \"section_definition\".", contextToken);
		}

		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("Exactly one argument should be here, specifying the name of the field to retrieve", contextToken);
			string name = (TokenParser.VerifyUnderlyingType(args[0].Expression.Evaluate(state, args[0].Token)) ?? "").ToString();
			EditFieldInfo info;
			if (!fieldsByName.TryGetValue(name, out info))
				throw new InstructionExecutionException("The field \"" + name + "\" does not exist for the referenced page.", args[0].Token);
			return info;
		}
	}
}
