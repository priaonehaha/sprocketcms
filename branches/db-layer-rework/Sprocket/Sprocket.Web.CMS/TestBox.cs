using System;
using System.Collections.Generic;
using System.Text;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Utility;
using Sprocket.Web.CMS;
using Sprocket.SystemBase;
using Sprocket.Web.Controls;

namespace Sprocket.Web.CMS
{
	[ModuleRegistrationCode("TestBox")]
	[ModuleDependency("WebsiteAdmin")]
	[ModuleDependency("AjaxFormHandler")]
	class TestBox : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			((WebsiteAdmin)registry["WebsiteAdmin"]).OnAdminRequest += new WebsiteAdmin.AdminRequestHandler(TestBox_OnAdminRequest);
		}

		void TestBox_OnAdminRequest(AdminInterface admin, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			//admin.AddMainMenuLink(new AdminMenuLink("Test Box", WebUtility.MakePath("admin/testbox"), 1000));
			if (sprocketPath != "admin/testbox") return;
			handled.Set();
			admin.AddInterfaceScript(new RankedString(ResourceLoader.LoadTextResource(typeof(AjaxForm).Assembly, "Sprocket.Web.Controls.AjaxForm.js"), 0));
			admin.AddHeadSection(new RankedString(CSS, 0));
			admin.ContentHeading = "Test Box";
			admin.AddContentSection(new RankedString("blah", -1000));

			AjaxFormFieldBlock b = new AjaxFormFieldBlock("UserDetails", "Main User Details");
			b.Add(new AjaxFormStandardField(
				"Username",
				"Username",
				"<input type=\"text\" id=\"Username\" />",
				null,
				"function(value) { return value.length == 0 ? 'Please enter a username' : false }",
				true, 1));
			b.Add(new AjaxFormStandardField("First Name", "FirstName", "<input type=\"text\" />", null, "", true, 0));

			AjaxFormFieldBlock b2 = new AjaxFormFieldBlock("RandomCrap", "Random Crap");
			b2.Add(new AjaxFormField("stuff", null, null, -1));

			AjaxFormFieldBlockList bl = new AjaxFormFieldBlockList();
			bl.Add(b);
			bl.Add(b2);

			admin.AddContentSection(new RankedString(bl, 1001));
		}

		private string CSS
		{
			get
			{
				return "<style>\r\n" +
					".ajaxform-row .label { display:block; float:left; width:100px; }\r\n" +
					".ajaxform-row .error { color:red; font-weight:bold; padding-left:10px; }\r\n" +
					"</style>";
			}
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		private string regCode;
		public string RegistrationCode
		{
			get { return regCode; }
			set { regCode = value; }
		}

		int importance = 0;
		public int Importance
		{
			get { return importance; }
			set { importance = value; }
		}

		public string Title
		{
			get { return "Web CMS Test Box"; }
		}

		public string ShortDescription
		{
			get { return "Another test module. This one is for testing CMS features."; }
		}
	}
}
