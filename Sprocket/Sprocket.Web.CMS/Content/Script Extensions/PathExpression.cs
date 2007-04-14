using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Content.Expressions
{
	public class BasePathExpression : IExpression
	{
		public object Evaluate(ExecutionState state) { return WebUtility.BasePath; }
		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}
	}

	public class BasePathExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "basepath"; } }
		public IExpression Create() { return new BasePathExpression(); }
	}

	public class SprocketPathExpression : IExpression
	{
		public object Evaluate(ExecutionState state) { return SprocketPath.Value; }
		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}
	}

	public class SprocketPathExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "path"; } }
		public IExpression Create() { return new SprocketPathExpression(); }
	}
}
