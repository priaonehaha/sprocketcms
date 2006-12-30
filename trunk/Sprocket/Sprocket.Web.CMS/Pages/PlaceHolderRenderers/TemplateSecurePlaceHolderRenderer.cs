using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class TemplateSecurePlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = true;
			
			String[] arr = placeHolder.Expression.Split('|');
			if (arr.Length != 2)
				return "[TemplateSecure requires expression: loggedInTemplateName|loggedOutTemplateName (use relevant template names, or blank for none)]";
			string name = (WebAuthentication.Instance.IsLoggedIn ? arr[0] : arr[1]).Trim();
			if (name.Length == 0)
				return "";
			Template t = TemplateRegistry.Templates[name];
			string text = t.Text;
			foreach (PlaceHolder ph in t.PlaceHolders)
			{
				bool cache;
				text = text.Replace(ph.RawText, ph.Render(pageEntry, content, placeHolderStack, out cache));
				containsCacheableContent = containsCacheableContent && cache;
			}
			return text;
		}
	}
}
