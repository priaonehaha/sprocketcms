using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class PageEntryPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = true;
			switch (placeHolder.Expression.ToLower())
			{
				case "path":
					return WebUtility.MakeFullPath(pageEntry.Path);

				default:
					return "[The {pageEntry} renderer does not support the expression " + placeHolder.Expression + "]";
			}
		}
	}
}
