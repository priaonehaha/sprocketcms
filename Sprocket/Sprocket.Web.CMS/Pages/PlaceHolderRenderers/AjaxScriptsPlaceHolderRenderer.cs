using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sprocket.Utility;
using Sprocket.Web;

namespace Sprocket.Web.CMS.Pages
{
	public class AjaxScriptsPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = true;

			Type[] types;
			if (placeHolder.Expression.Trim().Length > 0)
			{
				string[] typeNames = placeHolder.Expression.Split(',');
				types = new Type[typeNames.Length];
				for (int i = 0; i < typeNames.Length; i++)
				{
					string typeName = typeNames[i].Trim();
					if (typeName == "")
						continue;
					
					RegisteredModule mod = Core.Instance[typeName];
					if(mod == null)
					{
						containsCacheableContent = false;
						return "alert('[Type " + typeName + " not found]');";
					}
					types[i] = mod.Module.GetType();
				}
			}
			else
				types = new Type[0];

			string scr =
				ResourceLoader.LoadTextResource(typeof(WebClientScripts).Assembly, "Sprocket.Web.javascript.generic.js")
				+ ResourceLoader.LoadTextResource(typeof(WebClientScripts).Assembly, "Sprocket.Web.javascript.json.js")
				+ ResourceLoader.LoadTextResource(typeof(WebClientScripts).Assembly, "Sprocket.Web.javascript.ajax.js")
				+ WebClientScripts.Instance.GetAjaxMethodsScript(types);
			if (WebClientScripts.CompressJavaScript)
				return JavaScriptCondenser.Condense(scr);
			else
				return scr;
		}
	}
}
