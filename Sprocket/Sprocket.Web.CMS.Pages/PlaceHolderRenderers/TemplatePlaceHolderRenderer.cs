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
			Template template = TemplateRegistry.Templates[placeHolder.Expression];
			if (template == null)
			{
				containsCacheableContent = false;
				return "[No template found named \"" + placeHolder.Expression + "\"]";
			}

			string text = template.Text;
			bool ccc = true;
			foreach (PlaceHolder ph in template.PlaceHolders)
			{
				bool check;
				text = text.Replace(ph.RawText, ph.Render(pageEntry, content, placeHolderStack, out check));
				ccc = ccc & check;
			}
			containsCacheableContent = ccc;
			return text;
		}
	}
}
