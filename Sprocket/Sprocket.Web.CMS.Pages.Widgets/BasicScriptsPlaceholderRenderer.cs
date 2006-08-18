using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Utility;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.SystemBase;

namespace Sprocket.Web.CMS.Pages.Widgets
{
	public class BasicScriptsPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, System.Xml.XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			containsCacheableContent = true;
			return WebClientScripts.Instance.BuildScriptTags();
		}
	}
}
