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

	public class SprocketPathExpression : IObjectExpression
	{
		IExpression expr = null;
		Token argToken = null, propertyToken = null;

		public object Evaluate(ExecutionState state)
		{
			if (propertyToken != null)
			{
				if (propertyToken.Value == "total_sections")
					return SprocketPath.Sections.Length;
			}
			if (expr == null)
			{
				return SprocketPath.Value;
			}
			else
			{
				object obj = expr.Evaluate(state);
				if (obj is int || obj is long || obj is short || obj is decimal || obj is double || obj is float)
				{
					int n = Convert.ToInt32(obj);
					if(n >= SprocketPath.Sections.Length)
						throw new InstructionExecutionException("The argument you specified for the \"path\" expression equates to the number " + n + ", which is too high. Remember, the first component in the path is index zero (0). The second is one (1), and so forth.", argToken);
					return SprocketPath.Sections[n];
				}
				else if (obj == null)
					return SprocketPath.Value;
				else
					throw new InstructionExecutionException("You specified an argument for the \"path\" expression, but it isn't equating to a number. I need a number to know which bit of the path you are referring to.", argToken);
			}
		}
		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			if(propertyToken != null && arguments.Count > 0)
				throw new TokenParserException("you have specified one or more arguments for the \"path\" expression, even though you specified a property as well. I'm not sure how to deal with that.", functionCallToken);
			if (arguments.Count > 1)
				throw new TokenParserException("the \"path\" expression must contain no more than one argument, and it should be a number specifying which querystring element you want, or a string (word) specifying the name of the querystring parameter you want.", functionCallToken);
			if (arguments.Count == 1)
			{
				expr = arguments[0].Expression;
				argToken = arguments[0].Token;
			}
		}

		public bool PrepareProperty(Token propertyToken, List<Token> tokens, ref int nextIndex)
		{
			this.propertyToken = propertyToken;
			switch (propertyToken.Value)
			{
				case "total_sections":
					return true;
				default:
					return false;
			}
		}
	}

	public class SprocketPathExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "path"; } }
		public IExpression Create() { return new SprocketPathExpression(); }
	}
}
