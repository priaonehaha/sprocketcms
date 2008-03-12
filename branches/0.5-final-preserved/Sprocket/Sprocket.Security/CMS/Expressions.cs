using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web;
using Sprocket.Web.CMS.Script;


namespace Sprocket.Security.CMS
{
	class LoggedInExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return WebAuthentication.IsLoggedIn;
		}
	}
	class LoggedInExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "loggedin"; }
		}

		public IExpression Create()
		{
			return new LoggedInExpression();
		}
	}

	class UsernameExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (!WebAuthentication.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.Username;
		}
	}
	class UsernameExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "username"; }
		}

		public IExpression Create()
		{
			return new UsernameExpression();
		}
	}

	class AccountActivatedExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (!WebAuthentication.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.Activated;
		}
	}
	class AccountActivatedExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "accountactivated"; }
		}

		public IExpression Create()
		{
			return new AccountActivatedExpression();
		}
	}

	class UserIDExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (!WebAuthentication.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.UserID;
		}
	}
	class UserIDExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "userid"; }
		}

		public IExpression Create()
		{
			return new UserIDExpression();
		}
	}

	class EmailChangePendingExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (!WebAuthentication.IsLoggedIn)
				return "[not logged in]";
			if (CurrentRequest.Value["EmailChangePendingExpression_Value"] != null)
				return (bool)CurrentRequest.Value["EmailChangePendingExpression_Value"];
			EmailChangeRequest ecr = SecurityProvider.DataLayer.SelectEmailChangeRequest(SecurityProvider.CurrentUser.UserID);
			bool val;
			if (ecr == null)
				val = false;
			else
				val = true;
			CurrentRequest.Value["EmailChangePendingExpression_Value"] = val;
			return val;
		}
	}
	class EmailChangePendingExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "emailchangepending"; }
		}

		public IExpression Create()
		{
			return new EmailChangePendingExpression();
		}
	}

	class EmailAddressExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (!WebAuthentication.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.Email;
		}
	}
	class EmailAddressExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "emailaddress"; }
		}

		public IExpression Create()
		{
			return new EmailAddressExpression();
		}
	}

	class HasPermissionExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("\"haspermission\" expects exactly one argument specifying the name of a permission.", contextToken);
			object o = TokenParser.ReduceFromExpression(state, args[0].Token, args[0].Expression.Evaluate(state, args[0].Token));
			string permission = o == null ? null : o.ToString();
			if (permission == null)
				throw new InstructionExecutionException("the argument didn't equate to a string naming a permission.", contextToken);
			if (!WebAuthentication.IsLoggedIn)
				return false;
			return SecurityProvider.CurrentUser.HasPermission(permission);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("\"haspermission\" expects an argument specifying which name of a permission.", contextToken);
		}
	}
	class HasPermissionExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "haspermission"; }
		}

		public IExpression Create()
		{
			return new HasPermissionExpression();
		}
	}
}
