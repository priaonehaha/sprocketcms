using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

using Sprocket;
using Sprocket.Web;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Pages
{
	public partial class PageRequestHandler
	{
		void OnAdminRequest(AdminInterface admin, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			admin.AddMainMenuLink(new AdminMenuLink("Website Pages", WebUtility.MakeFullPath("admin/pages"), 0));

			if(handled.Handled) return;

			switch (sprocketPath)
			{
				case "admin/pages":
					admin.ContentHeading = "Website Page List";
					admin.AddContentSection(new RankedString(GetPageList(), 0));
					break;

				default:
					return;
			}

			handled.Set();
		}

		string GetPageList()
		{
			return "";
		}
	}
}
