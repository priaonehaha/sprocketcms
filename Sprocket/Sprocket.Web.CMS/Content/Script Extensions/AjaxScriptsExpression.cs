using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class AjaxScriptsExpression : IExpression
	{
		IExpression expr = null;
		Token token = null;

		public object Evaluate(ExecutionState state)
		{
			Type[] types;
			if (expr != null)
			{
				string[] typeNames = expr.Evaluate(state).ToString().Split(',');
				types = new Type[typeNames.Length];
				for (int i = 0; i < typeNames.Length; i++)
				{
					string typeName = typeNames[i].Trim();
					if (typeName == "")
						continue;

					RegisteredModule mod = Core.Instance[typeName];
					if (mod == null)
					{
						return "alert('[Type " + typeName + " not found]');";
					}
					types[i] = mod.Module.GetType();
				}
			}
			else
				types = new Type[0];

			System.Diagnostics.Debug.WriteLine("Rendering AJAX scripts with timestamp of " + AjaxRequestHandler.Instance.PageTimeStamp.Ticks.ToString());
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

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			token = tokens[index];
			expr = TokenParser.BuildExpression(tokens, ref index, precedenceStack, true);
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
