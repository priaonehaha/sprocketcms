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
using Sprocket.Web.FileManager;

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
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
			AdminHandler.AddPagePreprocessor("PageEdit", PreEditPage);
		}

		void WebEvents_OnLoadRequestedPath(HandleFlag handled)
		{
			if (handled.Handled) return;
			if (!WebAuthentication.IsLoggedIn) return;
			if (!WebAuthentication.VerifyAccess(PermissionType.ModifyPages)) return;

			if (SprocketPath.Sections.Length >= 4 && SprocketPath.Sections[0] == "admin")
			{
				switch (SprocketPath.Sections[1])
				{
					case "pages":
						switch (SprocketPath.Sections[2])
						{
							case "delete":
								{
									long id;
									if (long.TryParse(SprocketPath.Sections[3], out id))
									{
										Page page = ContentManager.Instance.DataProvider.SelectPage(id);
										if (page != null)
										{
											Result r = page.SaveRevision("** Page deleted.", page.RevisionInformation.Draft, page.RevisionInformation.Hidden, true);
											if (!r.Succeeded)
											{
												Response.Write("Unable to delete page:<br/>" + r.Message);
												Response.End();
												return;
											}
										}
									}
								}
								WebUtility.Redirect("admin/pages");
								break;

							case "imgthumb":
								{
									long id;
									if (long.TryParse(SprocketPath.Sections[3], out id))
									{
										SizingOptions options = new SizingOptions(60, 45, SizingOptions.Display.Constrain, id);
										FileManager.FileManager.Instance.TransmitImage(options);
									}
								}
								break;
						}
						break;
				}
			}
		}

		void AdminHandler_OnLoadAdminPage(AdminInterface admin, PageEntry page, HandleFlag handled)
		{
			if (WebAuthentication.VerifyAccess(PermissionType.ModifyPages))
				admin.AddMainMenuLink(new AdminMenuLink("Pages and Content", WebUtility.MakeFullPath("admin/pages"), ObjectRank.Normal, "pages_and_content"));
			if (WebAuthentication.VerifyAccess(PermissionType.ModifyTemplates))
				admin.AddMainMenuLink(new AdminMenuLink("Page Templates", WebUtility.MakeFullPath("admin/templates"), ObjectRank.Normal, "page_templates"));
		}

		void PreEditPage(PageEntry pe)
		{
			if (Request.Form.Count == 0)
			{
				Page page = null;
				if (SprocketPath.Sections.Length <= 3)
					page = new Page(0, 0, "Untitled Page", "", "", "", true, "", "text/html", DateTime.UtcNow, null);
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
			bool wasDraft = false;
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
			else if (pageID != 0)
				if (page.RevisionInformation.Draft)
					wasDraft = true;

			// load the page's existing content for comparison against newly-submitted content
			List<PreparedPageAdminSection> sectionlist = page.AdminSectionList;

			string prevTemplateName = page.TemplateName;
			Template prevTemplate = ContentManager.Templates[prevTemplateName];

			// read in the selected page categories and cause an error if any requirements were not satisfied
			Dictionary<string, List<string>> pagecategories = new Dictionary<string, List<string>>();
			if (prevTemplate != null)
			{
				foreach (CategorySet catset in prevTemplate.CategorySets)
				{
					if (catset.AllowMultiple)
					{
						List<string> cats = new List<string>();
						if (Request.Form["_$CategorySet_" + catset.Name] != null)
							foreach (string val in Request.Form.GetValues("_$CategorySet_" + catset.Name))
								cats.Add(val);

						foreach (Category cat in catset.Categories)
							if (cats.Contains(cat.Text))
							{
								List<string> catlist;
								if (!pagecategories.TryGetValue(catset.Name, out catlist))
								{
									catlist = new List<string>();
									pagecategories.Add(catset.Name, catlist);
								}
								catlist.Add(cat.Text);
							}

						if (!pagecategories.ContainsKey(catset.Name) && catset.Required)
							result.Merge(new Result("You must make at least one selection from the \"" + catset.Name + "\" category."));
					}
					else
					{
						string val = Request.Form["_$CategorySet_" + catset.Name];
						bool found = false;
						foreach (Category cat in catset.Categories)
							if (cat.Text == val)
							{
								found = true;
								break;
							}
						if (!found && catset.Required)
							result.Merge(new Result("You must make a selection from the \"" + catset.Name + "\" category."));
						else if (found)
						{
							List<string> catlist;
							if (!pagecategories.TryGetValue(catset.Name, out catlist))
							{
								catlist = new List<string>();
								pagecategories.Add(catset.Name, catlist);
							}
							catlist.Add(val);
						}
					}
				}
			}
			page.CategorySelections = pagecategories;

			result.Merge(page.ValidateFormField("PageName"));
			result.Merge(page.ValidateFormField("Requestable"));
			result.Merge(page.ValidateFormField("RequestPath"));
			result.Merge(page.ValidateFormField("TemplateName"));
			result.Merge(page.ValidateFormField("ContentType"));
			result.Merge(page.ValidateFormField("PublishDate"));
			result.Merge(page.ValidateFormField("ExpiryDate"));
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
							if (prevTemplateName != String.Empty && page.PageID != 0 && sectionlist.Count > 0)
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
										if (result.Succeeded)
											result.Merge(ContentManager.Instance.DataProvider.StoreEditFieldInfo(page.RevisionID, fld));
										if (result.Succeeded && isdifferent)
											result.Merge(fld.DataHandler.SaveData(fld.DataID, fld.Data));
									}
								}
							}
							// if this was a draft page and now it's not, delete the old draft revisions as they're no longer required
							if (result.Succeeded && wasDraft && !page.RevisionInformation.Draft)
								result = ContentManager.Instance.DataProvider.DeleteDraftRevisions(page.PageID);

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
			if (success)
				WebUtility.Redirect("admin/pages/edit/" + page.PageID + "/?saved");
		}

		public enum PermissionType
		{
			ModifyPages = 0,
			ModifyTemplates = 1
		}
	}
}
