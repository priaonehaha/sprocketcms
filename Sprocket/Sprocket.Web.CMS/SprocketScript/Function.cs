using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.SprocketScript.Parser;

namespace Sprocket.Web.CMS.SprocketScript
{
	public interface IFunction
	{
		
	}

	public interface IFunctionCreator
	{
		string Keyword { get; }
		IFunction Create();
	}

	public class HeadsOrTailsExpression : IExpression
	{
		private string hot = "heads";
		public object Evaluate(ExecutionState state)
		{
			Random r = new Random();
			int n = r.Next(0, 2);
			return new BooleanExpression.SoftBoolean(n == (hot == "heads" ? 0 : 1));
		}

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			Token token = tokens[index];
			if (token.TokenType == TokenType.StringLiteral && (token.Value.ToLower() == "heads" || token.Value.ToLower() == "tails"))
			{
				hot = token.Value.ToLower();
				index++;
			}
		}
	}

	public class HeadsOrTailsExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "headsortails"; } }

		public IExpression Create()
		{
			return new HeadsOrTailsExpression();
		}
	}
}
