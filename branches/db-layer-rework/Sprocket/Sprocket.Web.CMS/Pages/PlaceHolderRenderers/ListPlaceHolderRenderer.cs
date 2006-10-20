using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class ListPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			ListDefinition list = ListRegistry.Lists[placeHolder.Expression];
			if (list == null)
			{
				containsCacheableContent = false;
				return "[No list found named " + placeHolder.Expression + "]";
			}
			return list.Render(placeHolderStack, out containsCacheableContent);
		}
	}
}
