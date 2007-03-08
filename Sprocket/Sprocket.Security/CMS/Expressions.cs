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

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
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
}
