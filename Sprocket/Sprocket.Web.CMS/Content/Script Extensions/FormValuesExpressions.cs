using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket.Utility;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public static class FormValues
	{
		private const string key = "Sprocket.Web.CMS.Content.FormValues.Details";
		private static Dictionary<string, FormFieldInfo> Details
		{
			get
			{
				if (CurrentRequest.Value[key] == null)
					CurrentRequest.Value[key] = new Dictionary<string, FormFieldInfo>();
				return (Dictionary<string, FormFieldInfo>)CurrentRequest.Value[key];
			}
		}

		public static void Set(string name, string message, object value, bool isError)
		{
			FormFieldInfo ed = new FormFieldInfo();
			ed.Name = name;
			ed.Value = value;
			ed.Message = message;
			ed.IsError = isError;
			Details[name] = ed;
		}
		public static FormFieldInfo Get(string fieldName)
		{
			if (HasValue(fieldName))
				return Details[fieldName];
			return null;
		}
		public static bool IsError(string fieldName)
		{
			if (Details.ContainsKey(fieldName))
				return Details[fieldName].IsError;
			return false;
		}
		public static bool HasValue(string fieldName)
		{
			return Details.ContainsKey(fieldName);
		}
		public static bool HasError
		{
			get
			{
				foreach (FormFieldInfo info in Details.Values)
					if (info.IsError)
						return true;
				return false;
			}
		}
		public static int Count
		{
			get { return Details.Count; }
		}
		public static Dictionary<string, FormFieldInfo>.ValueCollection All
		{
			get
			{
				return Details.Values;
			}
		}

		public class FormFieldInfo : IPropertyEvaluatorExpression
		{
			public string Name, Message;
			public object Value;
			public bool IsError = false;

			public bool IsValidPropertyName(string propertyName)
			{
				switch (propertyName)
				{
					case "name":
					case "message":
					case "value":
					case "iserror":
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
						return Name;
					case "message":
						return Message;
					case "value":
						return Value;
					case "iserror":
						return IsError;
					default:
						return null;
				}
			}

			public object Evaluate(ExecutionState state, Token contextToken)
			{
				return Value;
			}

			public override bool Equals(object obj)
			{
				return obj == Value;
			}
		}
	}

	public class FormFieldExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if(args.Count != 2)
				throw new InstructionExecutionException("The FormField expression must take two arguments. The first is the name of the field, "
					+ "the second is the default value to use if no value has been preset for that field name.", contextToken);
			string fieldname = args[0].Expression.Evaluate(state,args[0].Token) as string;
			if (FormValues.HasValue(fieldname))
				return FormValues.Get(fieldname);
			FormValues.FormFieldInfo fld = new FormValues.FormFieldInfo();
			fld.IsError = false;
			fld.Message = "";
			fld.Name = fieldname;
			fld.Value = args[1].Expression.Evaluate(state, args[1].Token);
			return fld;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("The FormField expression must take two arguments. The first is the name of the field, "
				+ "the second is the default value to use if no value has been preset for that field name.", contextToken);
		}
	}
	class FormFieldExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "formfield"; }
		}

		public IExpression Create()
		{
			return new FormFieldExpression();
		}
	}

	public class IsFormErrorExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			foreach (ExpressionArgument arg in args)
			{
				string s = args[0].Expression.Evaluate(state, arg.Token) as string;
				if (FormValues.IsError(s))
					return true;
			}
			return false;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return FormValues.HasError;
		}
	}
	class IsFormErrorExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "isformerror"; }
		}

		public IExpression Create()
		{
			return new IsFormErrorExpression();
		}
	}

	public class FormErrorExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			foreach (ExpressionArgument arg in args)
			{
				string s = args[0].Expression.Evaluate(state, arg.Token) as string;
				if (FormValues.IsError(s))
					return FormValues.Get(s).Message;
			}
			return "";
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return FormValues.HasError ? "The form has one or more errors" : "There are no errors in the form";
		}
	}
	class FormErrorExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "formerror"; }
		}

		public IExpression Create()
		{
			return new FormErrorExpression();
		}
	}

	public class CheckedAttributeExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if(args.Count != 2)
				throw new InstructionExecutionException("CheckedAttribute requires two arguments; the name of the field and the default check state (true/false)", contextToken);
			string fieldname = args[0].Expression.Evaluate(state, args[0].Token) as string;
			if (FormValues.HasValue(fieldname))
			{
				object s = FormValues.Get(fieldname).Value;
				return BooleanExpression.False.Equals(s) ? "" : " checked";
			}
			object o = args[1].Expression.Evaluate(state, args[1].Token);
			return BooleanExpression.True.Equals(o) ? " checked" : "";
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("CheckedAttribute requires two arguments; the name of the field and the default check state (true/false)", contextToken);
		}
	}
	class CheckedAttributeExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "checked"; }
		}

		public IExpression Create()
		{
			return new CheckedAttributeExpression();
		}
	}
}
