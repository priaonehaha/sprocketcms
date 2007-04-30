using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.Script.Parser
{
	public class CurrentDateTime : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return SprocketDate.Now;
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
}
