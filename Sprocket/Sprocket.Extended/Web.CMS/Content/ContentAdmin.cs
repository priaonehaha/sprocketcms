using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Transactions;

using Sprocket.Web.CMS.Script;
using Sprocket.Utility;
using Sprocket.Web.CMS.Admin;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Content
{
	[ModuleTitle("Content Admin")]
	[ModuleDescription("The content management engine that handles content, pages and the templates they use")]
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(ContentManager))]
	[AjaxMethodHandler("ContentAdmin")]
	public sealed class ContentAdmin : ISprocketModule
	{
		public static ContentAdmin Instance
		{
			get { return (ContentAdmin)Core.Instance[typeof(ContentAdmin)].Module; }
		}

		HttpRequest Request { get { return HttpContext.Current.Request; } }
		HttpResponse Response { get { return HttpContext.Current.Response; } }

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			AdminHandler.Instance.OnLoadAdminPage += new AdminHandler.AdminRequestHandler(AdminHandler_OnLoadAdminPage);
			WebEvents.AddFormProcessor(new WebEvents.FormPostAction(null, null, "page-edit-button", "Save Page", SavePage));
			AdminHandler.AddPagePreprocessor("PageEdit", PreEditPage);
		}

		void AdminHandler_OnLoadAdminPage(AdminInterface admin, PageEntry page, HandleFlag handled)
		{
			if (WebAuthentication.VerifyAccess(PermissionType.ModifyPages))
				admin.AddMainMenuLink(new AdminMenuLink("Pages and Content", WebUtility.MakeFullPath("admin/pages"), ObjectRank.Normal));
			if (WebAuthentication.VerifyAccess(PermissionType.ModifyTemplates))
				admin.AddMainMenuLink(new AdminMenuLink("Page Templates", WebUtility.MakeFullPath("admin/templates"), ObjectRank.Normal));
		}

		void PreEditPage(PageEntry pe)
		{
			if (Request.Form.Count == 0)
			{
				Page page = null;
				if (SprocketPath.Sections.Length <= 3)
					page = new Page(0, 0, "Untitled Page", "", "", "", true, "", "text/html");
				else
				{
					long pageID;
					if (long.TryParse(SprocketPath.Sections[3], out pageID))
					{
						try
						{
							DatabaseManager.DatabaseEngine.GetConnection();
							page = ContentManager.Instance.DataProvider.SelectPage(pageID);
							FormValues.Set("Revision", null, page.RevisionInformation, false);
							FormValues.Set("AllRevisions", null, page.RevisionInformation, false);
						}
						finally
						{
							DatabaseManager.DatabaseEngine.ReleaseConnection();
						}
					}
				}
				FormValues.Set("Page", page == null ? "The requested page was not found." : null, page, page == null);
			}
		}

		void SavePage()
		{
			Result result = new Result();
			long pageID = -1;
			long.TryParse(Request.Form["PageID"], out pageID);
			Page page;
			if (pageID == 0)
				page = new Page();
			else
				page = ContentManager.Instance.DataProvider.SelectPage(pageID);
			if (page == null)
			{
				WebUtility.Redirect("admin/pages/edit/notfound");
				return;
			}

			// load the page's existing content for comparison against newly-submitted content
			List<PreparedPageAdminSection> sectionlist = page.AdminSectionList;

			string prevTemplate = page.TemplateName;
			result.Merge(page.ValidateFormField("PageName"));
			result.Merge(page.ValidateFormField("Requestable"));
			result.Merge(page.ValidateFormField("RequestPath"));
			result.Merge(page.ValidateFormField("TemplateName"));
			result.Merge(page.ValidateFormField("ContentType"));
			FormValues.Set("Page", result.Message, page, !result.Succeeded);


			bool success = false;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					DatabaseManager.DatabaseEngine.GetConnection();

					if (result.Succeeded)
					{
						if (page.PageID == 0)
							result = page.SaveRevision("Created new page \"" + page.PageName + "\".", true, false, false);
						else
							result = page.SaveRevision(Request.Form["RevisionNotes"].Trim(),
								StringUtilities.BoolFromString(Request.Form["Draft"]),
								StringUtilities.BoolFromString(Request.Form["Hidden"]),
								StringUtilities.BoolFromString(Request.Form["Deleted"]));
						if (result.Succeeded)
						{
							// now save all fields of the selected template. use the previously-selected template for field selection
							// as it will reflect any fields that were available in the form in that has just been submitted. if the
							// page is newly-created or has just come from a state where no template was selected, then there will be
							// no fields and thus we can skip this step.
							if (prevTemplate != String.Empty && page.PageID != 0 && sectionlist.Count > 0)
							{
								foreach (PreparedPageAdminSection section in sectionlist)
								{
									int rank = 0;
									foreach (EditFieldInfo fld in section.FieldList)
									{
										if (fld.DataHandler == null)
											continue;

										// todo: implement optional validation and thus use the returned Result value from ReadAdminField,
										// combined with FormValues.Set() and associated expressions in the page editor html file.
										IEditFieldData data;
										fld.Handler.ReadAdminField(out data);
										bool isdifferent = fld.Handler.IsContentDifferent(fld.Data, data) || fld.DataID == 0;
										if (isdifferent)
										{
											fld.Data = data;
											fld.DataID = 0;
										}
										fld.Rank = rank++;
										// we stil loop through each field to make sure the values are read from the form. if a failure has
										// occurred at some point though, the following checks ensure we don't bother trying to hit the db.
										if(result.Succeeded)
											result.Merge(ContentManager.Instance.DataProvider.StoreEditFieldInfo(page.RevisionID, fld));
										if(result.Succeeded && isdifferent)
											result.Merge(fld.DataHandler.SaveData(fld.DataID, fld.Data));
									}
								}
							}
							if (result.Succeeded)
							{
								success = true;
								scope.Complete();
							}
							else
								FormValues.Set("Page", result.Message, null, true);
						}
					}
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			if(success)
				WebUtility.Redirect("admin/pages/edit/" + page.PageID + "/?saved");
		}

		public enum PermissionType
		{
			ModifyPages = 0,
			ModifyTemplates = 1
		}
	}
}
