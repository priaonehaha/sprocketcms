using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Security.CMS
{
	class LoggedInExpression : IExpression
	{
		public object Evaluate(ExecutionState state)
		{
			return WebAuthentication.Instance.IsLoggedIn;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
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
		public object Evaluate(ExecutionState state)
		{
			if (!WebAuthentication.Instance.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.Username;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
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
		public object Evaluate(ExecutionState state)
		{
			if (!WebAuthentication.Instance.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.Activated;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
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
		public object Evaluate(ExecutionState state)
		{
			if (!WebAuthentication.Instance.IsLoggedIn)
				return "[not logged in]";
			return SecurityProvider.CurrentUser.UserID;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
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
		public object Evaluate(ExecutionState state)
		{
			if (!WebAuthentication.Instance.IsLoggedIn)
				return "[not logged in]";
			if (CurrentRequest.Value["EmailChangePendingExpression_Value"] != null)
				return (bool)CurrentRequest.Value["EmailChangePendingExpression_Value"];
			EmailChangeRequest ecr = SecurityProvider.Instance.DataLayer.SelectEmailChangeRequest(SecurityProvider.CurrentUser.UserID);
			bool val;
			if (ecr == null)
				val = false;
			else
				val = true;
			CurrentRequest.Value["EmailChangePendingExpression_Value"] = val;
			return val;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
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
}
