using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.Script.Parser
{
	public class CurrentDateTime : IExpression
	{
		public object Evaluate(ExecutionState state)
		{
			return DateTime.Now;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}
	}
	public class CurrentDateTimeCreator : IExpressionCreator
	{
		public string Keyword { get { return "currentdate"; } }
		public IExpression Create() { return new CurrentDateTime(); }
	}
	public class CurrentDateTimeCreator2 : IExpressionCreator
	{
		public string Keyword { get { return "currentdatetime"; } }
		public IExpression Create() { return new CurrentDateTime(); }
	}
	public class CurrentDateTimeCreator3 : IExpressionCreator
	{
		public string Keyword { get { return "now"; } }
		public IExpression Create() { return new CurrentDateTime(); }
	}

	public class DateFormatFilter : IFilterExpression
	{
		public object Evaluate(IExpression expr, ExecutionState state)
		{
			object o = expr.Evaluate(state);
			if (!(o is DateTime))
				throw new InstructionExecutionException("I can't format the date/time value here because the value doesn't seem to be a proper date/time.", token);
			string fmt = fmtExpr.Evaluate(state).ToString();
			try
			{
				return ((DateTime)o).ToString(fmt);
			}
			catch
			{
				throw new InstructionExecutionException("I can't format the date/time value here because the specified date format (" + fmt + ") isn't in a form that I understand", token);
			}
		}

		public object Evaluate(ExecutionState state)
		{
			throw new InstructionExecutionException("Evaluate is not supposed to be called in this way", token);
		}

		IExpression fmtExpr;
		Token token;
		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
			int n = nextIndex;
			fmtExpr = TokenParser.BuildExpression(tokens, ref nextIndex, precedenceStack);
			token = tokens[n];
		}
	}
	public class DateFormatFilterCreator : IFilterExpressionCreator
	{
		public string Keyword { get { return "dateformat"; } }
		public IFilterExpression Create() { return new DateFormatFilter(); }
	}
}
