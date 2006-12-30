using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class PathPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = true;
			if (placeHolder.Expression == "")
				return WebUtility.BasePath;
			PageEntry p = PageRegistry.Pages.FromPageCode(placeHolder.Expression);
			if(p == null)
				return "[Path requested for nonexistant page code: " + placeHolder.Expression + "]";
			return WebUtility.MakeFullPath(p.Path);
		}
	}
}
