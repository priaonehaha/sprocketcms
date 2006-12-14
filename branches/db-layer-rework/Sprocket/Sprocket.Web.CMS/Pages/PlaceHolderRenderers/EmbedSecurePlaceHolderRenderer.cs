using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class EmbedSecurePlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = false;
			String[] arr = placeHolder.Expression.Split('|');
			if (arr.Length != 2)
				return "[EmbedSecure requires expression: loggedInPageCode|loggedOutPageCode (use relevant page code values, or blank for none)]";

			string pagecode = (WebAuthentication.Instance.IsLoggedIn ? arr[0] : arr[1]).Trim();
			if (pagecode == "")
				return "";

			PageEntry page = PageRegistry.Pages.FromPageCode(pagecode);
			if (page == null)
				return "[No page found with PageCode \"" + pagecode + "\"]";

			bool discard;
			return page.Render(placeHolderStack, out discard);
		}
	}
}
