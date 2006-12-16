using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Security;
using Sprocket.Web.CMS.Pages;

namespace Sprocket.Web.CMS.Security
{
	public class CurrentUserPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, System.Xml.XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = false;
			if (!WebAuthentication.Instance.IsLoggedIn)
				return "[not logged in]";
			switch (placeHolder.Expression)
			{
				case "username":
					return SecurityProvider.CurrentUser.Username;

				default:
					return "[\"" + placeHolder.Expression + "\" is not a recognised placeholder expression for currentuser]";
			}
		}
	}
}
