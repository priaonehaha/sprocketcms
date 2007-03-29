using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS;
using Sprocket.Web.CMS.Content;

namespace Sprocket.Web.Forums
{
	[ModuleTitle("Forums Module")]
	[ModuleDescription("Provides discussion forum functionality")]
	//[AjaxMethodHandler("ForumHandler")]
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDependency(typeof(DatabaseManager))]
	public class ForumHandler : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(DatabaseManager_OnDatabaseHandlerLoaded);
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
	}
}
