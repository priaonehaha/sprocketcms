using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	internal class PropertyOfExpression : IExpression
	{
		private IExpression expr;
		private Token token; 

		public PropertyOfExpression(IExpression expr, Token token)
		{
			this.expr = expr;
			this.token = token;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (expr is IPropertyEvaluatorExpression)
				return ((IPropertyEvaluatorExpression)expr).EvaluateProperty(token.Value, token, state);
			object o = expr.Evaluate(state, token);
			if(o is IPropertyEvaluatorExpression)
				return ((IPropertyEvaluatorExpression)o).EvaluateProperty(token.Value, token, state);
			return SystemTypeEvaluator.EvaluateProperty(o, token.Value, token);
		}
	}
}
