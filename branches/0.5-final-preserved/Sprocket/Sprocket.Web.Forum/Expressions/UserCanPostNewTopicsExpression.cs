using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Sprocket;
using Sprocket.Web;
using Sprocket.Web.CMS;
using Sprocket.Security;
using Sprocket.Web.CMS.Content;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.Forums
{
	public class UserCanPostNewTopicsExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (CurrentRequest.Value["UserCanPostNewTopicsExpression_Value"] != null)
				return (bool)CurrentRequest.Value["UserCanPostNewTopicsExpression_Value"];

			ReferencedForum rf;
			if (CurrentRequest.Value["ReferencedForum_Value"] == null)
			{
				rf = ReferencedForum.ExtractFromURL();
				CurrentRequest.Value["ReferencedForum_Value"] = rf;
			}
			else
				rf = (ReferencedForum)CurrentRequest.Value["ReferencedForum_Value"];

			bool result;
			if (rf.Forum.PostNewTopics == Forum.AccessType.AllowAnonymous)
				result = true;

			else if (!WebAuthentication.IsLoggedIn)
				result = false;

			else
				switch (rf.Forum.PostNewTopics)
				{
					case Forum.AccessType.AllMembers:
						result = true;
						break;

					case Forum.AccessType.ActivatedMembers:
						result = SecurityProvider.CurrentUser.Activated;
						break;

					case Forum.AccessType.Administrators:
						result = SecurityProvider.CurrentUser.HasPermission(PermissionType.SuperUser);
						break;

					default:
						if (!rf.Forum.ModeratorRoleID.HasValue)
							result = false;
						else
							result = SecurityProvider.CurrentUser.HasRole(rf.Forum.ModeratorRoleID.Value);
						break;
				}
			CurrentRequest.Value["UserCanPostNewTopicsExpression_Value"] = result;
			return result;
		}
	}
	public class UserCanPostNewTopicsExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "user_can_post_new_topics"; } }
		public IExpression Create() { return new UserCanPostNewTopicsExpression(); }
	}
}
