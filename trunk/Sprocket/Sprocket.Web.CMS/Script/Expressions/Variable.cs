using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	public class VariableExpression : IFlexibleSyntaxExpression
	{
		private Token variableToken = null;
		private List<ExpressionArgument> arguments = null;
		private ExpressionProperty property = null;

		public Token VariableToken
		{
			get { return variableToken; }
		}

		public void PrepareExpression(TokenList tokens, Stack<int?> precedenceStack)
		{
			// get the token representing the variable name
			variableToken = tokens.Current;
			tokens.Advance();

			// build an optional argument list
			arguments = TokenParser.BuildArgumentList(tokens);

			// build an optional property name
			property = TokenParser.BuildPropertyExpression(tokens);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			// ensure that the variable has been set to some value first
			if (!state.Variables.ContainsKey(variableToken.Value))
				throw new InstructionExecutionException("I can't evaluate the word \"" + variableToken.Value + "\". Either it doesn't mean anything or you forgot to assign it a value.", variableToken);

			object o = state.Variables[variableToken.Value];
			if (o == null)
				return null;

			string type = o == null ? "null" : o.GetType().FullName;

			// if the value is a simple type (i.e. not an IExpression) ensure that it has no arguments and no property.
			if (!(o is IExpression))
			{
				if (arguments.Count > 0)
					throw new InstructionExecutionException("A list of arguments cannot be applied to the value held by this variable. (Internal variable type: " + type + ")", variableToken);
				if (property != null)
					o = SystemTypeEvaluator.EvaluateProperty(o, property.Name, property.PropertyToken);
				return o;
			}

			// if the expression can process argument lists and we've got at least one argument, process accordingly.
			if (arguments.Count > 0)
			{
				if (o is IArgumentListEvaluatorExpression)
					o = ((IArgumentListEvaluatorExpression)o).Evaluate(variableToken, arguments, state);
				else
					throw new InstructionExecutionException("A list of arguments cannot be applied to the value held by this variable. (Internal variable type: " + type + ")", variableToken);
			}

			// if the expression can evaluate a property and we've got a property specified, process accordingly
			if (property != null)
			{
				if (o is IPropertyEvaluatorExpression)
					if(!((IPropertyEvaluatorExpression)o).IsValidPropertyName(property.Name))
						throw new InstructionExecutionException("\"" + property.Name + "\" is not one of the allowed properties for this value. (Internal variable type: " + type + ")", property.PropertyToken);
					else
						o = property.EvaluateFor((IExpression)o, state);
				else
					o = SystemTypeEvaluator.EvaluateProperty(o, property.Name, property.PropertyToken);
			}

			if (o is IList)
				return new ListExpression((IList)o);

			return o;
		}
	}
}
