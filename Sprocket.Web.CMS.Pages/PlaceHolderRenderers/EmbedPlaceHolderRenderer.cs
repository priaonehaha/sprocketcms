using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class EmbedPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			PageEntry page = PageRegistry.Pages.FromPageCode(placeHolder.Expression);
			if (page == null)
			{
				containsCacheableContent = false;
				return "[No page found with PageCode \"" + placeHolder.Expression + "\"]";
			}
			return page.Render(placeHolderStack, out containsCacheableContent);
		}
	}
}
