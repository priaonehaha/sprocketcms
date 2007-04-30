using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Content.Expressions
{
	abstract class StringModifierBaseExpression : IArgumentListEvaluatorExpression, IFlexibleSyntaxExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ExpressionArgument arg in args)
			{
				object o = arg.Expression.Evaluate(state, arg.Token);
				if (o == null)
					sb.Append("null");
				else
					sb.Append(Adjust(o.ToString()));
			}
			return sb.ToString();
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("This expression requires at least one argument.", token);
		}
	
		Token token;
		public void PrepareExpression(TokenList tokens, Stack<int?> precedenceStack)
		{
 			token = tokens.Current;
			tokens.Advance();
		}

		protected abstract string Adjust(string s);
	}

	class HtmlEncodeExpression : StringModifierBaseExpression
	{
		protected override string Adjust(string s)
		{
			return HttpUtility.HtmlEncode(s);
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

	class SafeHtmlEncodeExpression : StringModifierBaseExpression
	{
		protected override string Adjust(string s)
		{
			return WebUtility.SafeHtmlString(s, false);
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

	class UrlEncodeExpression : StringModifierBaseExpression
	{
		protected override string Adjust(string s)
		{
			return WebUtility.UrlEncode(s);
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

	class LowerCaseExpression : StringModifierBaseExpression
	{
		protected override string Adjust(string s)
		{
			return s.ToLower();
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

	class UpperCaseExpression : StringModifierBaseExpression
	{
		protected override string Adjust(string s)
		{
			return s.ToUpper();
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
