using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using Sprocket.Web.CMS.Script;


namespace Sprocket.Web.FileManager.ScriptExtensions
{
	public class GetImageOptionsFilenameExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("At the very least, one argument is required specifying the id of an image in the database.", contextToken);
		}

		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if(args.Count == 0)
				throw new InstructionExecutionException("At the very least, one argument is required specifying the id of an image in the database.", contextToken);
			long id;
			if(!long.TryParse(args[0].Expression.Evaluate(state, args[0].Token).ToString(), out id))
				throw new InstructionExecutionException("The first argument must be a valid integer representing the id of an image in the database.", args[0].Token);
			SizingOptions options = new SizingOptions(0, 0, SizingOptions.Display.Constrain, id);
			options.SuspendFilenameUpdate(true);
			if (args.Count > 1)
			{
				int width;
				if (!int.TryParse(args[1].Expression.Evaluate(state, args[1].Token).ToString(), out width))
					throw new InstructionExecutionException("The second argument must be a valid integer representing the desired width (use 0 to scale relative to the height).", args[1].Token);
				options.Width = width;
			}
			if (args.Count > 2)
			{
				int height;
				if (!int.TryParse(args[1].Expression.Evaluate(state, args[2].Token).ToString(), out height))
					throw new InstructionExecutionException("The third argument must be a valid integer representing the desired height (use 0 to scale relative to the width).", args[2].Token);
				options.Width = height;
			}
			options.SuspendFilenameUpdate(false);
			return options.Filename;
		}
	}
	public class GetImageOptionsFilenameExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "get_image_options_filename"; } }
		public IExpression Create() { return new GetImageOptionsFilenameExpression(); }
	}
}
