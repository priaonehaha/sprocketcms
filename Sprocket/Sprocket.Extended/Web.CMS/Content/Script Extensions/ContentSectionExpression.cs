using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;

using System.Xml;

namespace Sprocket.Web.CMS.Content.Expressions
{
	public class ContentSectionExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("The content_section expression requires exactly one argument specifying the name of the content to render.", contextToken);
			object argval = args[0].Expression.Evaluate(state, args[0].Token);
			if (argval == null)
				throw new InstructionExecutionException("The argument specified equates to null. A non-null value is required in order to retrieve the desired content section.", args[0].Token);
			Dictionary<string, StringBuilder> content = CurrentRequest.Value["ContentSectionExpression.content"] as Dictionary<string, StringBuilder>;
			if (content == null) return String.Empty;
			StringBuilder sb;
			if (content.TryGetValue(argval.ToString().ToLower(), out sb))
				return sb.ToString();
			return String.Empty;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("The content_section expression requires an argument specifying the name of the content to render.", contextToken);
		}
	}
	public class ContentSectionExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "content_section"; } }
		public IExpression Create() { return new ContentSectionExpression(); }
	}
}
