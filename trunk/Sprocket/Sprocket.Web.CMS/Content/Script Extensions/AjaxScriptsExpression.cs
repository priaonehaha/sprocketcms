using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class AjaxScriptsExpression : IFunctionExpression
	{
		List<FunctionArgument> arguments = null;
		Token token = null;

		public object Evaluate(ExecutionState state)
		{
			Type[] types = new Type[arguments.Count];
			int c = 0;
			foreach (FunctionArgument arg in arguments)
			{
				IExpression expr = arg.Expression;
				object o = expr.Evaluate(state);
				if (o == null) o = String.Empty;
				string typeName = o.ToString();
				RegisteredModule mod = Core.Instance[typeName];
				if (mod == null)
				{
					return "alert('[Type " + typeName + " not found]');";
				}
				types[c++] = mod.Module.GetType();
			}

			//System.Diagnostics.Debug.WriteLine("Rendering AJAX scripts with timestamp of " + AjaxRequestHandler.Instance.PageTimeStamp.Ticks.ToString());
			string scr =
				ResourceLoader.LoadTextResource(typeof(WebClientScripts).Assembly, "Sprocket.Web.javascript.generic.js")
				+ ResourceLoader.LoadTextResource(typeof(WebClientScripts).Assembly, "Sprocket.Web.javascript.json.js")
				+ ResourceLoader.LoadTextResource(typeof(WebClientScripts).Assembly, "Sprocket.Web.javascript.ajax.js")
					.Replace("$APPLICATIONROOT$", WebUtility.BasePath)
					.Replace("$LOADTIMESTAMP$", AjaxRequestHandler.Instance.PageTimeStamp.Ticks.ToString())
				+ WebClientScripts.Instance.GetAjaxMethodsScript(types);
			if (WebClientScripts.CompressJavaScript)
				return JavaScriptCondenser.Condense(scr);
			else
				return scr;
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			this.arguments = arguments;
			token = functionCallToken;
		}
	}

	class AjaxScriptsExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "ajaxscripts"; }
		}

		public IExpression Create()
		{
			return new AjaxScriptsExpression();
		}
	}
}
