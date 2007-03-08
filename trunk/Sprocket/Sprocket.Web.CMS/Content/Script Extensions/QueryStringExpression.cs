using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class QueryStringExpression : IExpression
	{
		IExpression expr = null;
		Token token = null;

		public object Evaluate(ExecutionState state)
		{
			if (expr == null)
			{
				return WebUtility.RawQueryString;
			}
			else
			{
				object obj = expr.Evaluate(state);
				if (obj is int || obj is long || obj is short)
					return HttpContext.Current.Request.QueryString[Convert.ToInt32(obj)];
				else if (obj == null)
					return WebUtility.RawQueryString;
				else
					return HttpContext.Current.Request.QueryString[obj.ToString()];
			}
		}

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			//if(!tokens[index].IsNonScriptText)
			//    expr = TokenParser.BuildExpression(tokens, ref index, precedenceStack, true);
		}
	}

	class QueryStringExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "querystring"; }
		}

		public IExpression Create()
		{
			return new QueryStringExpression();
		}
	}
}
