using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content
{
	public class RevisionInformation: IJSONEncoder, IJSONReader, IPropertyEvaluatorExpression
	{
		#region Constructor, Fields, Properties, JSON Methods
		#region Fields

		protected long revisionID = 0;
		protected long revisionSourceID = 0;
		protected DateTime revisionDate = DateTime.UtcNow;
		protected long userID = 0;
		protected string notes = "";
		protected bool hidden = false;
		protected bool draft = false;
		protected bool deleted = false;

		#endregion

		#region Properties

		///<summary>
		///Gets or sets the value for RevisionID
		///</summary>
		public long RevisionID
		{
			get { return revisionID; }
			set { revisionID = value; }
		}

		///<summary>
		///Gets or sets the value for RevisionSourceID
		///</summary>
		public long RevisionSourceID
		{
			get { return revisionSourceID; }
			set { revisionSourceID = value; }
		}

		///<summary>
		///Gets or sets the value for RevisionDate
		///</summary>
		public DateTime RevisionDate
		{
			get { return revisionDate; }
			set { revisionDate = value; }
		}

		///<summary>
		///Gets or sets the value for UserID
		///</summary>
		public long UserID
		{
			get { return userID; }
			set { userID = value; }
		}

		///<summary>
		///Gets or sets the value for Notes
		///</summary>
		public string Notes
		{
			get { return notes; }
			set { notes = value; }
		}

		///<summary>
		///Gets or sets the value for Hidden
		///</summary>
		public bool Hidden
		{
			get { return hidden; }
			set { hidden = value; }
		}

		///<summary>
		///Gets or sets the value for Draft
		///</summary>
		public bool Draft
		{
			get { return draft; }
			set { draft = value; }
		}

		///<summary>
		///Gets or sets the value for Deleted
		///</summary>
		public bool Deleted
		{
			get { return deleted; }
			set { deleted = value; }
		}

		#endregion

		#region Constructors

		public RevisionInformation()
		{
		}

		public RevisionInformation(long revisionID, long revisionSourceID, DateTime revisionDate, long userID, string notes, bool hidden, bool draft, bool deleted)
		{
			this.revisionID = revisionID;
			this.revisionSourceID = revisionSourceID;
			this.revisionDate = revisionDate;
			this.userID = userID;
			this.notes = notes;
			this.hidden = hidden;
			this.draft = draft;
			this.deleted = deleted;
		}

		public RevisionInformation(IDataReader reader)
		{
			if (reader["RevisionID"] != DBNull.Value) revisionID = (long)reader["RevisionID"];
			if (reader["RevisionSourceID"] != DBNull.Value) revisionSourceID = (long)reader["RevisionSourceID"];
			if (reader["RevisionDate"] != DBNull.Value) revisionDate = (DateTime)reader["RevisionDate"];
			if (reader["UserID"] != DBNull.Value) userID = (long)reader["UserID"];
			if (reader["Notes"] != DBNull.Value) notes = (string)reader["Notes"];
			if (reader["Hidden"] != DBNull.Value) hidden = (bool)reader["Hidden"];
			if (reader["Draft"] != DBNull.Value) draft = (bool)reader["Draft"];
			if (reader["Deleted"] != DBNull.Value) deleted = (bool)reader["Deleted"];
		}

		#endregion

		#region Clone
		public RevisionInformation Clone()
		{
			RevisionInformation copy = new RevisionInformation();
			copy.revisionID = revisionID;
			copy.revisionSourceID = revisionSourceID;
			copy.revisionDate = revisionDate;
			copy.userID = userID;
			copy.notes = notes;
			copy.hidden = hidden;
			copy.draft = draft;
			copy.deleted = deleted;
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
			JSON.EncodeNameValuePair(writer, "RevisionID", revisionID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "RevisionSourceID", revisionSourceID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "RevisionDate", revisionDate);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UserID", userID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Notes", notes);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Hidden", hidden);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Draft", draft);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Deleted", deleted);
			writer.Write("}");
		}

		public void LoadJSON(object json)
		{
			Dictionary<string, object> values = json as Dictionary<string, object>;
			if (values == null) return;
			revisionID = (long)values["RevisionID"];
			revisionSourceID = (long)values["RevisionSourceID"];
			revisionDate = (DateTime)values["RevisionDate"];
			userID = (long)values["UserID"];
			notes = (string)values["Notes"];
			hidden = (bool)values["Hidden"];
			draft = (bool)values["Draft"];
			deleted = (bool)values["Deleted"];
		}

		#endregion

		#endregion

		#region Expression Methods
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return "[RevisionInformation: " + revisionID + "]";
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "revisionid":
				case "revisionsourceid":
				case "revisiondate":
				case "userid":
				case "notes":
				case "hidden":
				case "draft":
				case "deleted":
					return true;
				default:
					return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "revisionid": return RevisionID;
				case "revisionsourceid": return RevisionSourceID;
				case "revisiondate": return RevisionDate;
				case "userid": return UserID;
				case "notes": return Notes;
				case "hidden": return Hidden;
				case "draft": return Draft;
				case "deleted": return Deleted;
				default: return null;
			}
		}
		#endregion
	}
}