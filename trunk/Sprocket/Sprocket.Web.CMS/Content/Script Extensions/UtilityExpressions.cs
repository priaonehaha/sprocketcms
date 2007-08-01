using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket.Utility;
using Sprocket.Web.CMS.Script;

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
				while (o is IExpression)
				{
					if (object.ReferenceEquals(o, arg.Expression))
						break;
					o = ((IExpression)o).Evaluate(state, arg.Token);
				}
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

	class HowLongAgoExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("how_long_ago expects exactly one argument specifying which date to evaluate.", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			if (o == null)
				return "Never";
			if (o is DateTime)
				return StringUtilities.ApproxHowLongAgo(SprocketDate.Now, (DateTime)o);
			throw new InstructionExecutionException("the argument being fed to how_long_ago turned out to evaluate to something other than a date. (Underlying type: " + (o == null ? "null" : o.GetType().Name) + ")", contextToken);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("how_long_ago expects an argument specifying which date to evaluate.", contextToken);
		}
	}
	class HowLongAgoExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "how_long_ago"; }
		}

		public IExpression Create()
		{
			return new HowLongAgoExpression();
		}
	}

	class FormatDateExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 2)
				throw new InstructionExecutionException("\"formatdate\" expects two arguments; one specifying which date to evaluate, then one specifying the format string.", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			if (o is DateTime)
			{
				object s = args[1].Expression.Evaluate(state, args[1].Token);
				if (s is string)
				{
					try
					{
						return ((DateTime)o).ToString((string)s);
					}
					catch (Exception ex)
					{
						throw new InstructionExecutionException("The date format you specified was not valid. Try Googling for \"custom .Net DateTime formats\".", ex, args[1].Token);
					}
				}
			}
			else if (o == null)
				return null;
			throw new InstructionExecutionException("the first argument being fed to \"formatdate\" turned out to evaluate to something other than a date. (Underlying type: " + (o == null ? "null" : o.GetType().Name) + ")", contextToken);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("\"formatdate\" expects an argument specifying which date to evaluate.", contextToken);
		}
	}
	class FormatDateExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "formatdate"; }
		}

		public IExpression Create()
		{
			return new FormatDateExpression();
		}
	}

	class FormatNumberExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 2)
				throw new InstructionExecutionException("\"formatnumber\" expects two arguments; one specifying which date to evaluate, then one specifying the format string.", contextToken);
			object o = TokenParser.VerifyUnderlyingType(TokenParser.ReduceFromExpression(state, args[0].Token, args[0].Expression.Evaluate(state, args[0].Token)));
			if (o is decimal)
			{
				object s = args[1].Expression.Evaluate(state, args[1].Token);
				if (s is string)
				{
					try
					{
						return ((decimal)o).ToString((string)s);
					}
					catch (Exception ex)
					{
						throw new InstructionExecutionException("The number format you specified was not valid. Try Googling for \"custom .Net number formats\".", ex, args[1].Token);
					}
				}
			}
			throw new InstructionExecutionException("the first argument being fed to \"formatnumber\" turned out to evaluate to something other than a number. (Underlying type: " + (o == null ? "null" : o.GetType().Name) + ")", contextToken);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("\"formatnumber\" expects an argument specifying which date to evaluate.", contextToken);
		}
	}
	class FormatNumberExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "formatnumber"; }
		}

		public IExpression Create()
		{
			return new FormatNumberExpression();
		}
	}

	class CurrentDateExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return SprocketDate.Now;
		}
	}
	class CurrentDateExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "currentdate"; }
		}

		public IExpression Create()
		{
			return new CurrentDateExpression();
		}
	}

	class MonthNameExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("how_long_ago expects exactly one argument specifying which month number to evaluate.", contextToken);
			object o = TokenParser.VerifyUnderlyingType(args[0].Expression.Evaluate(state, args[0].Token));
			if (o is decimal)
				if ((decimal)o > 12 || (decimal)o < 1)
					throw new InstructionExecutionException("the argument being fed to monthname turned out to evaluate to " + o + ". Valid values are between 1 and 12.", args[0].Token);
				else
					return new DateTime(2001, Convert.ToInt32(o), 1).ToString("MMMM");
			throw new InstructionExecutionException("the argument being fed to monthname turned out to evaluate to something other than an integer. (Underlying type: " + (o == null ? "null" : o.GetType().Name) + ")", args[0].Token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("monthname expects an argument specifying a number between 1 and 12.", contextToken);
		}
	}
	class MonthNameExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "monthname"; }
		}

		public IExpression Create()
		{
			return new MonthNameExpression();
		}
	}

	class IsNullExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("\"isnull\" expects exactly one argument specifying the value to check.", contextToken);
			object o = TokenParser.ReduceFromExpression(state, args[0].Token, args[0].Expression.Evaluate(state, args[0].Token));
			return o == null;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("\"isnull\" expects an argument specifying which value to check.", contextToken);
		}
	}
	class IsNullExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "isnull"; }
		}

		public IExpression Create()
		{
			return new IsNullExpression();
		}
	}

	class IsNumberExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("\"isnumber\" expects exactly one argument specifying the value to check.", contextToken);
			object o = TokenParser.VerifyUnderlyingType(TokenParser.ReduceFromExpression(state, args[0].Token, args[0].Expression.Evaluate(state, args[0].Token)));
			return o is decimal;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("\"isnumber\" expects an argument specifying which value to check.", contextToken);
		}
	}
	class IsNumberExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "isnumber"; }
		}

		public IExpression Create()
		{
			return new IsNumberExpression();
		}
	}

	class SpacesToDashesExpression : StringModifierBaseExpression
	{
		protected override string Adjust(string s)
		{
			return s.Replace(" ", "-");
		}
	}
	class SpacesToDashesExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "spaces_to_dashes"; }
		}

		public IExpression Create()
		{
			return new SpacesToDashesExpression();
		}
	}

	class JSONEncodeExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("jsonencode expects exactly one argument specifying something to encode.", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			return JSON.Encode(o);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("jsonencode expects an argument specifying something to encode.", contextToken);
		}
	}
	class JSONEncodeExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "jsonencode"; }
		}

		public IExpression Create()
		{
			return new JSONEncodeExpression();
		}
	}
}
