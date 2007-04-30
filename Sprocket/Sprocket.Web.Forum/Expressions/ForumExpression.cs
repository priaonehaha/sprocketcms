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
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.Forums
{
	public class ForumExpression : IPropertyExpression, IVariableExpression
	{
		private PropertyType propertyType = PropertyType.None;
		private List<ExpressionArgument> args = null;
		private Forum forum = null;

		public ForumExpression() { }
		public ForumExpression(Forum forum)
		{
			this.forum = forum;
		}

		public bool PresetPropertyName(Token propertyToken, List<Token> tokens, ref int nextIndex)
		{
			switch (propertyToken.Value)
			{
				case "forumid": propertyType = PropertyType.ForumID; return true;
				case "forumcategoryid": propertyType = PropertyType.ForumCategoryID; return true;
				case "forumcode": propertyType = PropertyType.ForumCode; return true;
				case "name": propertyType = PropertyType.Name; return true;
				case "urltoken": propertyType = PropertyType.URLToken; return true;
				case "datecreated": propertyType = PropertyType.DateCreated; return true;
				case "rank": propertyType = PropertyType.Rank; return true;
				case "writeaccess": propertyType = PropertyType.WriteAccess; return true;
				case "readaccess": propertyType = PropertyType.ReadAccess; return true;
				case "markuplevel": propertyType = PropertyType.MarkupLevel; return true;
				case "showsignatures": propertyType = PropertyType.ShowSignatures; return true;
				case "allowimagesinmessages": propertyType = PropertyType.AllowImagesInMessages; return true;
				case "allowimagesinsignatures": propertyType = PropertyType.AllowImagesInSignatures; return true;
				case "requiremoderation": propertyType = PropertyType.RequireModeration; return true;
				case "allowvoting": propertyType = PropertyType.AllowVoting; return true;
				case "topicdisplayorder": propertyType = PropertyType.TopicDisplayOrder; return true;
				case "locked": propertyType = PropertyType.Locked; return true;
				default: return false;
			}
		}

		public void SetFunctionArguments(List<ExpressionArgument> arguments, Token functionCallToken)
		{
			args = arguments;
			if (args.Count != 1)
				throw new TokenParserException("An argument was expected specifying which \"forum\" to load.", functionCallToken);
		}

		public object Evaluate(ExecutionState state)
		{
			// attempt to load the specified Forum
			object value = TokenParser.VerifyUnderlyingType(args[0].Expression.Evaluate(state));
			if (value is decimal)
				forum = ForumHandler.DataLayer.SelectForum((long)value);
			else if (value is string)
				forum = ForumHandler.DataLayer.SelectForumByCode((string)value);
			else
				throw new InstructionExecutionException("Could not load an instance of \"forum\" because the argument did not equate to the right kind of value.", args[0].Token);

			// return the relevant value
			switch (propertyType)
			{
				case PropertyType.ForumID: return forum.ForumID;
				case PropertyType.ForumCategoryID: return forum.ForumCategoryID;
				case PropertyType.ForumCode: return forum.ForumCode;
				case PropertyType.Name: return forum.Name;
				case PropertyType.URLToken: return forum.URLToken;
				case PropertyType.DateCreated: return forum.DateCreated;
				case PropertyType.Rank: return forum.Rank;
				case PropertyType.WriteAccess: return forum.WriteAccess;
				case PropertyType.ReadAccess: return forum.ReadAccess;
				case PropertyType.MarkupLevel: return forum.MarkupLevel;
				case PropertyType.ShowSignatures: return forum.ShowSignatures;
				case PropertyType.AllowImagesInMessages: return forum.AllowImagesInMessages;
				case PropertyType.AllowImagesInSignatures: return forum.AllowImagesInSignatures;
				case PropertyType.RequireModeration: return forum.RequireModeration;
				case PropertyType.AllowVoting: return forum.AllowVoting;
				case PropertyType.TopicDisplayOrder: return forum.TopicDisplayOrder;
				case PropertyType.Locked: return forum.Locked;
				case PropertyType.None: return this;
				default: return "[Forum]";
			}
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public object EvaluateVariableProperty(string propertyName, Token propertyToken, ExecutionState state)
		{
			switch (propertyName)
			{
				case "forumid": return forum.ForumID;
				case "forumcategoryid": return forum.ForumCategoryID;
				case "forumcode": return forum.ForumCode;
				case "name": return forum.Name;
				case "urltoken": return forum.URLToken;
				case "datecreated": return forum.DateCreated;
				case "rank": return forum.Rank;
				case "writeaccess": return forum.WriteAccess;
				case "readaccess": return forum.ReadAccess;
				case "markuplevel": return forum.MarkupLevel;
				case "showsignatures": return forum.ShowSignatures;
				case "allowimagesinmessages": return forum.AllowImagesInMessages;
				case "allowimagesinsignatures": return forum.AllowImagesInSignatures;
				case "requiremoderation": return forum.RequireModeration;
				case "allowvoting": return forum.AllowVoting;
				case "topicdisplayorder": return forum.TopicDisplayOrder;
				case "locked": return forum.Locked;
				default:
					throw new InstructionExecutionException("\"" + propertyName + "\" is not a property of this variable.", propertyToken);
			}
		}

		enum PropertyType
		{
			None,
			ForumID,
			ForumCategoryID,
			ForumCode,
			Name,
			URLToken,
			DateCreated,
			Rank,
			WriteAccess,
			ReadAccess,
			MarkupLevel,
			ShowSignatures,
			AllowImagesInMessages,
			AllowImagesInSignatures,
			RequireModeration,
			AllowVoting,
			TopicDisplayOrder,
			Locked
		}
	}
	public class ForumExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "forum1"; } }
		public IExpression Create() { return new ForumExpression(); }
	}
}
