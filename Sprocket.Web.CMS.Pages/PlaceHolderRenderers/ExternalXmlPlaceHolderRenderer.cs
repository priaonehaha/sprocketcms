using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class ExternalXmlPlaceHolderRenderer : XmlPlaceHolderRenderer
	{
		public override string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			int scindex = placeHolder.Expression.IndexOf(';');
			if (scindex == -1)
			{
				containsCacheableContent = false;
				return "[externalXml placeholder expression formatted improperly. Expected {externalXml:pagecode;xpath}]";
			}
			string code = placeHolder.Expression.Substring(0, scindex);
			string xpath = placeHolder.Expression.Substring(scindex + 1);

			PageEntry page = PageRegistry.Pages.FromPageCode(code);
			if (page == null)
			{
				containsCacheableContent = false;
				return "[No external page exists with code " + code + "]";
			}

			return Render(xpath, page, page.LoadContentDocument(), placeHolderStack, out containsCacheableContent);
		}
	}
}
