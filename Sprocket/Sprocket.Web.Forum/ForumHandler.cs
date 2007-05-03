using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS;
using Sprocket.Web.CMS.Content;
using Sprocket.Security;

namespace Sprocket.Web.Forums
{
	[ModuleTitle("Forums Module")]
	[ModuleDescription("Provides discussion forum functionality")]
	//[AjaxMethodHandler("ForumHandler")]
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(DatabaseManager))]
	public class ForumHandler : ISprocketModule
	{
		HttpRequest Request { get { return HttpContext.Current.Request; } }
		HttpResponse Response { get { return HttpContext.Current.Response; } }

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(DatabaseManager_OnDatabaseHandlerLoaded);
			ContentManager.Instance.OnBeforeRenderPage += new ContentManager.BeforeRenderPage(ContentManager_OnBeforeRenderPage);
		}

		void ContentManager_OnBeforeRenderPage(PageEntry page)
		{
			if (SprocketPath.Sections[SprocketPath.Sections.Length-1] == "$save_forum_settings")
			{
				switch (Request.Form["action"])
				{
					case "save_forum_settings": SaveForumSettings(); break;
				}
				Response.Redirect(Request.UrlReferrer.ToString());
			}
		}

		private IForumDataHandler dataLayer = null;
		public static IForumDataHandler DataLayer
		{
			get { return Instance.dataLayer; }
		}

		public static ForumHandler Instance
		{
			get { return (ForumHandler)Core.Instance[typeof(ForumHandler)].Module; }
		}

		void DatabaseManager_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			source.OnInitialise += new InterruptableEventHandler(DatabaseHandler_OnInitialise);
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IForumDataHandler)))
			{
				IForumDataHandler dal = (IForumDataHandler)Activator.CreateInstance(t);
				if (dal.DatabaseHandlerType == source.GetType())
				{
					dataLayer = dal;
					break;
				}
			}
		}

		void DatabaseHandler_OnInitialise(Result result)
		{
			if (!result.Succeeded)
				return;
			if (dataLayer == null)
				result.SetFailed("ForumHandler has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
				dataLayer.InitialiseDatabase(result);
		}

		void SaveForumSettings()
		{
			if (!SecurityProvider.CurrentUser.HasPermission(ForumPermissionType.ForumCreator))
				throw new Exception("You don't have permission to create, edit or delete forums.");

			Forum forum;
			ForumCategory cat;

			long forumID = long.Parse(Request.Form["forumid"]);
			string categoryCode = Request.Form["categorycode"];

			cat = DataLayer.SelectForumCategoryByCode(categoryCode);
			if (cat == null)
			{
				cat = new ForumCategory(DatabaseManager.GetUniqueID(), SecurityProvider.ClientSpaceID,
					categoryCode, categoryCode, null, SprocketDate.Now, 0, false);
				DataLayer.Store(cat);
			}
				
			if (forumID == 0)
			{
				forum = new Forum();
				forum.ForumID = DatabaseManager.GetUniqueID();
			}
			else
				forum = dataLayer.SelectForum(forumID);
			if (forum == null)
				return;

			forum.Name = Request.Form["forum-name"].Trim();
			if (forum.Name == "") forum.Name = "Untitled Forum";
			forum.URLToken = Request.Form["url-token"].Trim();
			if (forum.URLToken == "") forum.URLToken = forum.ForumID.ToString();
			forum.ForumCode = Request.Form["forum-code"].Trim();
			forum.ForumCategoryID = cat.ForumCategoryID;
			forum.TopicDisplayOrder = short.Parse(Request.Form["topic-display-order"]);
			forum.MarkupLevel = short.Parse(Request.Form["markuplevel"]);

			string postAccess = Request.Form["post-access"];
			string replyAccess = Request.Form["reply-access"];
			string readAccess = Request.Form["read-access"];

			if (postAccess.StartsWith("_"))
			{
				forum.PostWriteAccess = short.Parse(postAccess.Substring(1));
				forum.PostWriteAccessRoleID = null;
			}
			else
			{
				forum.PostNewTopics = Forum.AccessType.RoleMembers;
				forum.PostWriteAccessRoleID = long.Parse(postAccess);
			}

			if (replyAccess.StartsWith("_"))
			{
				forum.ReplyWriteAccess = short.Parse(replyAccess.Substring(1));
				forum.ReplyWriteAccessRoleID = null;
			}
			else
			{
				forum.WriteReplies = Forum.AccessType.RoleMembers;
				forum.ReplyWriteAccessRoleID = long.Parse(replyAccess);
			}

			if (readAccess.StartsWith("_"))
			{
				forum.ReadAccess = short.Parse(readAccess.Substring(1));
				forum.ReadAccessRoleID = null;
			}
			else
			{
				forum.Read = Forum.AccessType.RoleMembers;
				forum.ReadAccessRoleID = long.Parse(readAccess);
			}

			forum.ModeratorRoleID = long.Parse(Request.Form["moderator-role"]);
			forum.RequireModeration = Request.Form["requires-moderation"] == "1";
			forum.AllowVoting = Request.Form["allow-voting"] == "1";
			forum.AllowImagesInMessages = Request.Form["message-images"] == "1";
			forum.ShowSignatures = Request.Form["show-signatures"] == "1";
			forum.AllowImagesInSignatures = Request.Form["signature-images"] == "1";
			forum.Locked = Request.Form["lock-forum"] == "1";
			forum.DateCreated = SprocketDate.Now;

			Result r = dataLayer.Store(forum);
			if (!r.Succeeded)
				throw new Exception(r.Message);
		}
	}
}