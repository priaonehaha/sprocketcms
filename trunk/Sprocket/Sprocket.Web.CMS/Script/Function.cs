using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Script
{
	public class BoldLineExpression : IFunctionExpression
	{
		public object Evaluate(ExecutionState state)
		{
			string str = "";
			foreach (FunctionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state);
				string s;
				if (o == null)
					s = "null";
				else
					s = o.ToString();
				str += "<div style=\"font-weight:bold;\">" + s + "</div>";
			}
			return str;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		List<FunctionArgument> args = null;
		Token token = null;
		public void SetArguments(List<FunctionArgument> arguments, Token functionToken)
		{
			args = arguments;
			token = functionToken;
		}
	}

	public class BoldLineExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "boldline"; } }

		public IExpression Create()
		{
			return new BoldLineExpression();
		}
	}
}
