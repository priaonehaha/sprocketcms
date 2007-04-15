using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class QueryStringExpression : IFunctionExpression, IObjectListExpression
	{
		IExpression expr = null;
		Token argToken = null;

		public object Evaluate(ExecutionState state)
		{
			if (expr == null)
			{
				return WebUtility.RawQueryString;
			}
			else
			{
				object obj = expr.Evaluate(state);
				if (obj is int || obj is long || obj is short || obj is decimal || obj is double || obj is float)
					return HttpContext.Current.Request.QueryString[Convert.ToInt32(obj)];
				else if (obj == null)
					return WebUtility.RawQueryString;
				else
					return HttpContext.Current.Request.QueryString[obj.ToString()];
			}
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			if (arguments.Count > 1)
				throw new TokenParserException("the querystring expression must contain no more than one argument, and it should be a number specifying which querystring element you want, or a string (word) specifying the name of the querystring parameter you want.", functionCallToken);
			if (arguments.Count == 1)
			{
				expr = arguments[0].Expression;
				argToken = arguments[0].Token;
			}
		}

		public List<IIteratorObject> GetList(ExecutionState state)
		{
			List<IIteratorObject> list = new List<IIteratorObject>();
			foreach (string s in HttpContext.Current.Request.QueryString)
				list.Add(new QSComponent(s, HttpContext.Current.Request.QueryString[s]));
			return list;
		}

		private class QSComponent : IIteratorObject
		{
			public object EvaluateProperty(string propertyName, Token propertyToken, ExecutionState state)
			{
				switch (propertyName)
				{
					case "length": return value.Length;
					case "name": return name;
					default: throw new InstructionExecutionException("\"" + propertyName + "\" is not a property of this object", propertyToken);
				}
			}

			public object Evaluate(ExecutionState state)
			{
				return value;
			}

			public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
			{
			}

			string value, name;
			public QSComponent(string name, string value)
			{
				this.name = name;
				this.value = value;
			}
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
