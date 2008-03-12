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
	public class CurrentForumOrTopicPageExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			if (CurrentRequest.Value["CurrentForumOrTopicPageExpression_Value"] != null)
				return (int)CurrentRequest.Value["CurrentForumOrTopicPageExpression_Value"];

			ReferencedForum rf = ReferencedForum.Current;

			CurrentRequest.Value["CurrentForumOrTopicPageExpression_Value"] = rf.Page;
			return rf.Page;
		}
	}
	public class CurrentForumOrTopicPageExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "current_forum_or_topic_page"; } }
		public IExpression Create() { return new CurrentForumOrTopicPageExpression(); }
	}
}
