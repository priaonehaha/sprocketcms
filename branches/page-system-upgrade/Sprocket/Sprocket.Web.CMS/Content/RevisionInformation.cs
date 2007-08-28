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
		protected long revisionGroupID = 0;
		protected DateTime revisionDate = DateTime.MinValue;
		protected bool isCurrent = false;
		protected long userID = 0;
		protected string notes = null;
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
		///Gets or sets the value for RevisionGroupID
		///</summary>
		public long RevisionGroupID
		{
			get { return revisionGroupID; }
			set { revisionGroupID = value; }
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
		///Gets or sets the value for IsCurrent
		///</summary>
		public bool IsCurrent
		{
			get { return isCurrent; }
			set { isCurrent = value; }
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
		
		public RevisionInformation(long revisionID, long revisionGroupID, DateTime revisionDate, bool isCurrent, long userID, string notes, bool deleted)
		{
			this.revisionID = revisionID;
			this.revisionGroupID = revisionGroupID;
			this.revisionDate = revisionDate;
			this.isCurrent = isCurrent;
			this.userID = userID;
			this.notes = notes;
			this.deleted = deleted;
		}
		
		public RevisionInformation(IDataReader reader)
		{
			if(reader["RevisionID"] != DBNull.Value) revisionID = (long)reader["RevisionID"];
			if(reader["RevisionGroupID"] != DBNull.Value) revisionGroupID = (long)reader["RevisionGroupID"];
			if(reader["RevisionDate"] != DBNull.Value) revisionDate = (DateTime)reader["RevisionDate"];
			if(reader["IsCurrent"] != DBNull.Value) isCurrent = (bool)reader["IsCurrent"];
			if(reader["UserID"] != DBNull.Value) userID = (long)reader["UserID"];
			if(reader["Notes"] != DBNull.Value) notes = (string)reader["Notes"];
			if(reader["Deleted"] != DBNull.Value) deleted = (bool)reader["Deleted"];
		}

		#endregion
		
		#region Clone
		public RevisionInformation Clone()
		{
			RevisionInformation copy = new RevisionInformation();
			copy.revisionID = revisionID;
			copy.revisionGroupID = revisionGroupID;
			copy.revisionDate = revisionDate;
			copy.isCurrent = isCurrent;
			copy.userID = userID;
			copy.notes = notes;
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
			JSON.EncodeNameValuePair(writer, "RevisionID",revisionID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "RevisionGroupID",revisionGroupID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "RevisionDate",revisionDate);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "IsCurrent",isCurrent);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UserID",userID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Notes",notes);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Deleted",deleted);
			writer.Write("}");
		}
		
		public void LoadJSON(object json)
		{
			Dictionary<string, object> values = json as Dictionary<string, object>;
			if(values == null) return;
			revisionID = (long)values["RevisionID"];
			revisionGroupID = (long)values["RevisionGroupID"];
			revisionDate = (DateTime)values["RevisionDate"];
			isCurrent = (bool)values["IsCurrent"];
			userID = (long)values["UserID"];
			notes = (string)values["Notes"];
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
			switch(propertyName)
			{
				case "revisionid":
				case "revisiongroupid":
				case "revisiondate":
				case "iscurrent":
				case "userid":
				case "notes":
				case "deleted":
					return true;
				default:
					return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch(propertyName)
			{
				case "revisionid": return RevisionID;
				case "revisiongroupid": return RevisionGroupID;
				case "revisiondate": return RevisionDate;
				case "iscurrent": return IsCurrent;
				case "userid": return UserID;
				case "notes": return Notes;
				case "deleted": return Deleted;
				default: return null;
			}
		}
		#endregion
	}
}