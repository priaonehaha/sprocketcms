using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class TemplatePlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = true;
			Template t = TemplateRegistry.Templates[placeHolder.Expression];
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
