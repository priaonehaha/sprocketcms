using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Content
{
	public class RequestSpeedExpression : IExpression
	{
		private static DateTime start = DateTime.Now;
		public static void Set() { start = DateTime.Now; }

		public object Evaluate(ExecutionState state)
		{
			TimeSpan ts = DateTime.Now.Subtract(start);
			return ts.TotalSeconds.ToString("0.0##") + "s";
		}

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
		}
	}

	public class RequestSpeedExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "requestspeed"; } }
		public IExpression Create() { return new RequestSpeedExpression(); }
	}
}
