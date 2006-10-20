using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Text;
using Sprocket.SystemBase;
using Sprocket.Utility;

namespace Sprocket.Web
{
    /// <summary>
    /// Testing
    /// </summary>
	public class SystemBasePage : Page
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}
		/*
		protected bool Authenticate()
		{
			if(!Request.RawUrl.EndsWith("?logout"))
			{
				WebAuthentication auth = (WebAuthentication)SystemCore.Instance["WebAuthentication"];
				
				if(Session["AuthKey"] != null)
				{
					if(auth.GetUsername((Guid)Session["AuthKey"]).ToString() != "")
					{
						OnAuthCheckSucceeded();
						return true;
					}
				}
			}
			Session.Abandon();
			Response.Redirect("login.aspx", true);
			return false;
		}

		protected virtual void OnAuthCheckSucceeded()
		{
			((WebClientScripts)SystemCore.Instance["WebClientScripts"]).RenderClientScripts();
		}
		 * */
	}
}
