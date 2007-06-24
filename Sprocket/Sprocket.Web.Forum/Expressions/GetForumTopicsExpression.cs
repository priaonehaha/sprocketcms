using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket;
using Sprocket.Web;
using Sprocket.Web.CMS;
using Sprocket.Security;
using Sprocket.Web.CMS.Content;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.Forums
{
	public class GetForumTopicsExpression : IArgumentListEvaluatorExpression, IFlexibleSyntaxExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count == 0)
				throw new InstructionExecutionException("An argument was expected specifying which forum to load from.", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			if(o == null)
				throw new InstructionExecutionException("The first argument equates to null. It needs to equate to the forum's id.", args[0].Token);
			long id;
			try
			{
				if(o is string)
					id = long.Parse((string)o);
				else
					id = Convert.ToInt64(o);
			}
			catch
			{
				throw new InstructionExecutionException("The first argument needs to equate to a 64 bit number. (Underlying type: " + o.GetType().FullName + ")", args[0].Token);
			}
			int total;
			if (args.Count == 1)
			{
				List<ForumTopicSummary> list = ForumHandler.DataLayer.ListForumTopicSummary(id, WebAuthentication.IsLoggedIn ? (long?)SecurityProvider.CurrentUser.UserID : null,
					null, 0, 1, null, false, false, out total);
				return new ForumTopicPageSummary(total, 1, 0, list);
			}
			else
			{
				if (args.Count != 3)
					throw new InstructionExecutionException("Expected either one or three arguments: (forum_id) or (forum_id, current_page, topics_per_page)", contextToken);
				int page, perPage;
				o = args[1].Expression.Evaluate(state, args[1].Token);
				try
				{
					if (o is string) page = int.Parse((string)o);
					else page = Convert.ToInt32(o);
				}
				catch
				{
					throw new InstructionExecutionException("The second argument needs to equate to a number. (Underlying type: " + o.GetType().FullName + ")", args[1].Token);
				}
				o = args[2].Expression.Evaluate(state, args[2].Token);
				try
				{
					if (o is string) perPage = int.Parse((string)o);
					else perPage = Convert.ToInt32(o);
				}
				catch
				{
					throw new InstructionExecutionException("The third argument needs to equate to a number. (Underlying type: " + o.GetType().FullName + ")", args[2].Token);
				}
				List<ForumTopicSummary> list = ForumHandler.DataLayer.ListForumTopicSummary(
					id, WebAuthentication.IsLoggedIn ? (long?)SecurityProvider.CurrentUser.UserID : null,
					null, perPage, page, null, false, false, out total);
				return new ForumTopicPageSummary(total, page, perPage, list);
			}
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("You must specify an argument representing the forum id for the forum topics you want to load.", contextToken);
		}

		public void PrepareExpression(TokenList tokens, Stack<int?> precedenceStack)
		{
			if(tokens.Next.TokenType != TokenType.GroupStart)
				throw new TokenParserException("You must specify an argument representing the forum id for the forum topics you want to load.", tokens.Current);
			tokens.Advance();
		}
	}
	public class GetForumTopicsExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "get_forum_topics"; } }
		public IExpression Create() { return new GetForumTopicsExpression(); }
	}
}
