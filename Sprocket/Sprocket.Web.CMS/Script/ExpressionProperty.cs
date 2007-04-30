using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	/// <summary>
	/// Handles processing of expression properties. When the token parser finds a property designator (:), the
	/// token following it is presumed to be a property of the preceding expression. This class is invoked to
	/// handle building of the property and any sub-properties thereof.
	/// </summary>
	public class ExpressionProperty
	{
		private Token propertyToken = null, expressionToken = null;
		List<ExpressionArgument> arguments = new List<ExpressionArgument>();
		ExpressionProperty subProperty = null;

		public ExpressionProperty SubProperty
		{
			get { return subProperty; }
		}

		public string Name
		{
			get { return propertyToken.Value; }
		}

		public bool IsValidPropertyName(string propertyName)
		{
			return true;
		}

		public Token PropertyToken
		{
			get { return propertyToken; }
		}

		public ExpressionProperty(TokenList tokens)
		{
			if (tokens.Current.TokenType != TokenType.Word)
				throw new TokenParserException("I can't use this as a property of the preceding expression because it's not a word. A property needs to be formatted like this: expression_name:property_name", tokens.Current);
			expressionToken = tokens.Peek(-2);
			propertyToken = tokens.Current;
			tokens.Advance();
			arguments = TokenParser.BuildArgumentList(tokens);
			subProperty = TokenParser.BuildPropertyExpression(tokens);
		}

		public object EvaluateFor(IExpression expr, ExecutionState state)
		{
			IPropertyEvaluatorExpression pExpr = expr as IPropertyEvaluatorExpression;
			if(!pExpr.IsValidPropertyName(Name))
				throw new InstructionExecutionException("\"" + Name + "\" is not one of the allowed properties for this value.", propertyToken);
			object o = pExpr.EvaluateProperty(this, state);

			// if the expression can process argument lists and we've got at least one argument, process accordingly.
			if (arguments.Count > 0)
			{
				if (o is IArgumentListEvaluatorExpression)
					o = ((IArgumentListEvaluatorExpression)o).Evaluate(propertyToken, arguments, state);
				else
					throw new InstructionExecutionException("A list of arguments cannot be applied to the value held by this variable. (Internal variable type: " + o.GetType().FullName + ")", expressionToken);
			}

			if (subProperty != null)
				if (o is IPropertyEvaluatorExpression)
					return subProperty.EvaluateFor(o as IExpression, state);
				else
					return SystemTypeEvaluator.EvaluateProperty(o, subProperty.Name, subProperty.PropertyToken);

			return o;
		}
	}
}
