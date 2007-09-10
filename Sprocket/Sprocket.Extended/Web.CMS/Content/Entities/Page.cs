using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Transactions;
using System.Web;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Utility;
using Sprocket.Security;

namespace Sprocket.Web.CMS.Content
{
	public class Page : IJSONEncoder, IJSONReader, IPropertyEvaluatorExpression
	{
		#region Constructor, Fields, Properties, JSON Methods
		#region Fields

		protected long pageID = 0;
		protected long revisionID = 0;
		protected string pageName = "";
		protected string pageCode = "";
		protected string parentPageCode = "";
		protected string templateName = "";
		protected bool requestable = false;
		protected string requestPath = "";
		protected string contentType = "";

		#endregion

		#region Properties

		///<summary>
		///Gets or sets the value for PageID
		///</summary>
		public long PageID
		{
			get { return pageID; }
			set { pageID = value; }
		}

		///<summary>
		///Gets or sets the value for RevisionID
		///</summary>
		public long RevisionID
		{
			get { return revisionID; }
			set { revisionID = value; }
		}

		///<summary>
		///Gets or sets the value for PageName
		///</summary>
		public string PageName
		{
			get { return pageName; }
			set { pageName = value; }
		}

		///<summary>
		///Gets or sets the value for PageCode
		///</summary>
		public string PageCode
		{
			get { return pageCode; }
			set { pageCode = value; }
		}

		///<summary>
		///Gets or sets the value for ParentPageCode
		///</summary>
		public string ParentPageCode
		{
			get { return parentPageCode; }
			set { parentPageCode = value; }
		}

		///<summary>
		///Gets or sets the value for TemplateName
		///</summary>
		public string TemplateName
		{
			get { return templateName; }
			set { templateName = value; }
		}

		///<summary>
		///Gets or sets the value for Requestable
		///</summary>
		public bool Requestable
		{
			get { return requestable; }
			set { requestable = value; }
		}

		///<summary>
		///Gets or sets the value for RequestPath
		///</summary>
		public string RequestPath
		{
			get { return requestPath; }
			set { requestPath = value; }
		}

		///<summary>
		///Gets or sets the value for ContentType
		///</summary>
		public string ContentType
		{
			get { return contentType; }
			set { contentType = value; }
		}

		#endregion

		#region Constructors

		public Page()
		{
		}

		public Page(long pageID, long revisionID, string pageName, string pageCode, string parentPageCode, string templateName, bool requestable, string requestPath, string contentType)
		{
			this.pageID = pageID;
			this.revisionID = revisionID;
			this.pageName = pageName;
			this.pageCode = pageCode;
			this.parentPageCode = parentPageCode;
			this.templateName = templateName;
			this.requestable = requestable;
			this.requestPath = requestPath;
			this.contentType = contentType;
		}

		public Page(IDataReader reader)
		{
			if (reader["PageID"] != DBNull.Value) pageID = (long)reader["PageID"];
			if (reader["RevisionID"] != DBNull.Value) revisionID = (long)reader["RevisionID"];
			if (reader["PageName"] != DBNull.Value) pageName = (string)reader["PageName"];
			if (reader["PageCode"] != DBNull.Value) pageCode = (string)reader["PageCode"];
			if (reader["ParentPageCode"] != DBNull.Value) parentPageCode = (string)reader["ParentPageCode"];
			if (reader["TemplateName"] != DBNull.Value) templateName = (string)reader["TemplateName"];
			if (reader["Requestable"] != DBNull.Value) requestable = (bool)reader["Requestable"];
			if (reader["RequestPath"] != DBNull.Value) requestPath = (string)reader["RequestPath"];
			if (reader["ContentType"] != DBNull.Value) contentType = (string)reader["ContentType"];
		}

		#endregion

		#region Clone
		public Page Clone()
		{
			Page copy = new Page();
			copy.pageID = pageID;
			copy.revisionID = revisionID;
			copy.pageName = pageName;
			copy.pageCode = pageCode;
			copy.parentPageCode = parentPageCode;
			copy.templateName = templateName;
			copy.requestable = requestable;
			copy.requestPath = requestPath;
			copy.contentType = contentType;
			return copy;
		}
		#endregion

		#region JSON Methods

		/// <summary>
		/// Writes this entity out as a JSON formatted string
		/// </summary>
		public void WriteJSON(StringWriter writer)
		{
			writer.Write("{");
			JSON.EncodeNameValuePair(writer, "PageID", pageID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "RevisionID", revisionID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "PageName", pageName);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "PageCode", pageCode);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "ParentPageCode", parentPageCode);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "TemplateName", templateName);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Requestable", requestable);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "RequestPath", requestPath);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "ContentType", contentType);
			writer.Write("}");
		}

		public void LoadJSON(object json)
		{
			Dictionary<string, object> values = json as Dictionary<string, object>;
			if (values == null) return;
			pageID = (long)values["PageID"];
			revisionID = (long)values["RevisionID"];
			pageName = (string)values["PageName"];
			pageCode = (string)values["PageCode"];
			parentPageCode = (string)values["ParentPageCode"];
			templateName = (string)values["TemplateName"];
			requestable = (bool)values["Requestable"];
			requestPath = (string)values["RequestPath"];
			contentType = (string)values["ContentType"];
		}

		#endregion

		#endregion

		#region Expression Methods
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return "[Page: " + pageID + "]";
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "pageid":
				case "revisionid":
				case "pagename":
				case "pagecode":
				case "parentpagecode":
				case "templatename":
				case "requestable":
				case "requestpath":
				case "contenttype":
					return true;
				default:
					return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "pageid": return PageID;
				case "revisionid": return RevisionID;
				case "pagename": return PageName;
				case "pagecode": return PageCode;
				case "parentpagecode": return ParentPageCode;
				case "templatename": return TemplateName;
				case "requestable": return Requestable;
				case "requestpath": return RequestPath;
				case "contenttype": return ContentType;
				default: return null;
			}
		}
		#endregion

		private RevisionInformation revisionInformation = null;
		public RevisionInformation RevisionInformation
		{
			get
			{
				if (revisionInformation == null)
					if (pageID == 0 || revisionID == 0)
						return null;
					else
						revisionInformation = ContentManager.Instance.DataProvider.SelectRevisionInformation(revisionID);
				return revisionInformation;
			}
		}

		public Result SaveRevision(string notes, bool draft, bool hidden, bool deleted)
		{
			if (notes == null) notes = "";
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					DatabaseManager.DatabaseEngine.GetConnection();
					RevisionInformation rev = new RevisionInformation(0, 0, SprocketDate.Now, SecurityProvider.CurrentUser.UserID, notes, hidden, draft, deleted);
					rev.RevisionID = DatabaseManager.GetUniqueID();
					if (pageID == 0) pageID = DatabaseManager.GetUniqueID();
					rev.RevisionSourceID = pageID;
					revisionID = rev.RevisionID;
					Result r = ContentManager.Instance.DataProvider.Store(rev);
					if (r.Succeeded)
						r = ContentManager.Instance.DataProvider.Store(this);
					if (!r.Succeeded)
						return r;
					scope.Complete();
					revisionInformation = rev;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public Result ValidateFormField(string fieldName)
		{
			object value = null;
			string val = HttpContext.Current.Request.Form[fieldName] ?? "";
			Result result = new Result();
			switch (fieldName)
			{
				case "PageID": value = pageID = long.TryParse(val, out pageID) ? pageID : 0; break;
				case "RevisionID": value = revisionID = long.TryParse(val, out revisionID) ? pageID : 0; break;
				case "PageCode": value = pageCode = val.Trim(); break;
				case "PageName": value = pageName = (val == String.Empty ? "Untitled Page" : val).Trim(); break;
				case "Requestable": value = requestable = StringUtilities.BoolFromString(val); break;
				case "RequestPath":
					value = requestPath = val.Trim().Replace(" ", "-");
					if (!Uri.IsWellFormedUriString("http://localhost/" + requestPath + "/", UriKind.Absolute))
						result.SetFailed("The specified URL is badly formed. Make sure you haven't typed in any invalid characters.");
					break;
				case "TemplateName":
					value = templateName = val.Trim();
					if (templateName != String.Empty)
						if (ContentManager.Templates[templateName] == null)
							result.SetFailed("The template \"" + templateName + "\" is not valid. It may have been deleted.");
					break;
				case "ContentType": value = contentType = val.Trim(); break;
			}
			FormValues.Set(fieldName, result.Message, value, !result.Succeeded);
			return result;
		}

		public Dictionary<string, List<ContentNode>> LoadContentNodes()
		{
			Dictionary<string, List<ContentNode>> nodes = ContentManager.Instance.DataProvider.ListContentNodesForPage(revisionID);
			Dictionary<string, List<ContentNode>> nodesByFieldName = new Dictionary<string, List<ContentNode>>();
			foreach (KeyValuePair<string, List<ContentNode>> list in nodes)
			{
				foreach (ContentNode node in list.Value) // group all of the nodes (currently grouped according to node type) into field name groups
				{
					List<ContentNode> fieldnodes;
					if (!nodesByFieldName.TryGetValue(node.FieldName, out fieldnodes))
					{
						fieldnodes = new List<ContentNode>();
						nodesByFieldName.Add(node.FieldName, fieldnodes);
					}
					fieldnodes.Add(node);
				}
				IContentNodeDatabaseInterface dbi = ContentManager.GetNodeTypeDatabaseInterface(list.Key);
				if (dbi == null) continue;
				dbi.LoadNodeData(list.Value);
			}
			// now sort nodesByFieldName in ascending order, ready to be matched to the template admin fields
			foreach (List<ContentNode> list in nodesByFieldName.Values)
				list.Sort();

			/* the next step is to store nodesByFieldName as a property of this class and then when the
			 * admin for the page is displayed, each content field in the template admin definition should
			 * be compared against nodesByFieldName to get the existing data. */
			return nodesByFieldName;
		}
	}
}