using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	internal class PropertyOfExpression : IExpression
	{
		private IPropertyEvaluatorExpression expr;
		private ExpressionProperty property;
		private Token token;

		public PropertyOfExpression(IPropertyEvaluatorExpression expr, ExpressionProperty property, Token propertyNameToken)
		{
			this.expr = expr;
			this.property = property;
			this.token = propertyNameToken;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (expr is ArgumentsOfExpression)
			{
				object o = ((ArgumentsOfExpression)expr).Evaluate(state, token);
				if (o is IExpression)
					return property.EvaluateFor((IExpression)o, state);
				else
					return SystemTypeEvaluator.EvaluateProperty(o, property.Name, property.PropertyToken);
			}
			if (!expr.IsValidPropertyName(property.Name))
				throw new InstructionExecutionException("\"" + property.Name + "\" is not a valid property of this expression.", token);
			return property.EvaluateFor(expr, state);
		}
	}
}
