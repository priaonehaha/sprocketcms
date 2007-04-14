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
			throw new Exception("The method or operation is not implemented.");
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
			throw new Exception("The method or operation is not implemented.");
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
}
