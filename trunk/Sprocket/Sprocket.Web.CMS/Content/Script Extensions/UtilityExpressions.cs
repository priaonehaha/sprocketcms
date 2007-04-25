using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class HtmlEncodeExpression : IFunctionExpression
	{
		Token token = null;
		List<FunctionArgument> args = null;

		public object Evaluate(ExecutionState state)
		{
			StringBuilder sb = new StringBuilder();
			foreach (FunctionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state);
				if (o == null)
					sb.Append("null");
				else
					sb.Append(HttpUtility.HtmlEncode(o.ToString()));
			}
			return sb.ToString();
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			token = functionCallToken;
			if (arguments.Count == 0)
				throw new TokenParserException("The \"htmlencode\" function requires at least one argument.", token);
			args = arguments;
		}
	}
	class HtmlEncodeExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "htmlencode"; }
		}

		public IExpression Create()
		{
			return new HtmlEncodeExpression();
		}
	}

	class SafeHtmlEncodeExpression : IFunctionExpression
	{
		Token token = null;
		List<FunctionArgument> args = null;

		public object Evaluate(ExecutionState state)
		{
			StringBuilder sb = new StringBuilder();
			foreach (FunctionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state);
				if (o == null)
					sb.Append("null");
				else
					sb.Append(WebUtility.SafeHtmlString(o.ToString(), true));
			}
			return sb.ToString();
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			token = functionCallToken;
			if (arguments.Count == 0)
				throw new TokenParserException("The \"safehtmlencode\" function requires at least one argument.", token);
			args = arguments;
		}
	}
	class SafeHtmlEncodeExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "safehtmlencode"; }
		}

		public IExpression Create()
		{
			return new SafeHtmlEncodeExpression();
		}
	}

	class UrlEncodeExpression : IFunctionExpression
	{
		Token token = null;
		List<FunctionArgument> args = null;

		public object Evaluate(ExecutionState state)
		{
			StringBuilder sb = new StringBuilder();
			foreach (FunctionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state);
				if (o == null)
					sb.Append("null");
				else
					sb.Append(WebUtility.UrlEncode(o.ToString()));
			}
			return sb.ToString();
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			token = functionCallToken;
			if (arguments.Count == 0)
				throw new TokenParserException("The \"htmlencode\" function requires at least one argument.", token);
			args = arguments;
		}
	}
	class UrlEncodeExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "urlencode"; }
		}

		public IExpression Create()
		{
			return new UrlEncodeExpression();
		}
	}

	class LowerCaseExpression : IFunctionExpression
	{
		Token token = null;
		List<FunctionArgument> args = null;

		public object Evaluate(ExecutionState state)
		{
			StringBuilder sb = new StringBuilder();
			foreach (FunctionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state);
				if (o == null)
					sb.Append("null");
				else
					sb.Append(o.ToString().ToLower());
			}
			return sb.ToString();
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			token = functionCallToken;
			if (arguments.Count == 0)
				throw new TokenParserException("The \"lowercase\" function requires at least one argument.", token);
			args = arguments;
		}
	}
	class LowerCaseExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "lowercase"; }
		}

		public IExpression Create()
		{
			return new LowerCaseExpression();
		}
	}

	class UpperCaseExpression : IFunctionExpression
	{
		Token token = null;
		List<FunctionArgument> args = null;

		public object Evaluate(ExecutionState state)
		{
			StringBuilder sb = new StringBuilder();
			foreach (FunctionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state);
				if (o == null)
					sb.Append("null");
				else
					sb.Append(o.ToString().ToUpper());
			}
			return sb.ToString();
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			token = functionCallToken;
			if (arguments.Count == 0)
				throw new TokenParserException("The \"uppercase\" function requires at least one argument.", token);
			args = arguments;
		}
	}
	class UpperCaseExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "uppercase"; }
		}

		public IExpression Create()
		{
			return new UpperCaseExpression();
		}
	}
}
