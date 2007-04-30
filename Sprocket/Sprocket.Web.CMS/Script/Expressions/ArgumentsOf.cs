using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	internal class ArgumentsOfExpression : IPropertyEvaluatorExpression
	{
		private IArgumentListEvaluatorExpression expr;
		private List<ExpressionArgument> args;
		private Token precedingToken;

		public ArgumentsOfExpression(IArgumentListEvaluatorExpression expr, List<ExpressionArgument> args, TokenList tokens, Token precedingToken)
		{
			this.args = args;
			this.expr = expr;
			this.precedingToken = precedingToken;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return ((IArgumentListEvaluatorExpression)expr).Evaluate(precedingToken, args, state);
		}

		public bool IsValidPropertyName(string propertyName)
		{
			return true;
		}

		public object EvaluateProperty(ExpressionProperty prop, ExecutionState state)
		{
			return prop.EvaluateFor(expr, state);
		}
	}
}
