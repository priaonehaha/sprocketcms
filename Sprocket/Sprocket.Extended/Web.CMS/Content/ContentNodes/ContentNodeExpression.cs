using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content.ContentNodes
{
	class ContentNodeExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if(args.Count == 0)
				throw new InstructionExecutionException("The \"content\" expression requires an argument specifying the name of a content node to retrieve", contextToken);
			if(args.Count > 1)
				throw new InstructionExecutionException("The \"content\" expression takes a single argument specifying the name of a content node to retrieve. You've specified more than one argument.", contextToken);
			return "[todo: render content node output]";
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("The \"content\" expression requires an argument specifying the name of a content node to retrieve", contextToken);
		}
	}

	class ContentNodeExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "content"; } }
		public IExpression Create() { return new ContentNodeExpression(); }
	}
}
