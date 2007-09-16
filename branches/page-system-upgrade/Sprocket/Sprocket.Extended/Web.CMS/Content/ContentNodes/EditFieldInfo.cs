using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Sprocket.Utility;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	/// <summary>
	/// Holds information to link a piece of (usually) editable data on a page to the handler that allows
	/// that data to be manipulated. Also provides extra info to assist in matching the field to the correct
	/// location in the page edit form.
	/// </summary>
	public class EditFieldInfo : IComparable, IPropertyEvaluatorExpression
	{
		private IEditFieldHandler editFieldHandler;
		private IEditFieldHandlerDatabaseInterface dataHandler;
		private IEditFieldData data;
		private long editFieldID;
		private int rank;
		private string fieldName, sectionName;

		public string SectionName
		{
			get { return sectionName; }
		}

		public string FieldName
		{
			get { return fieldName; }
		}

		public int Rank
		{
			get { return rank; }
			set { rank = value; }
		}

		public long DataID
		{
			get { return editFieldID; }
			set { editFieldID = value; }
		}

		public IEditFieldHandler Handler
		{
			get { return editFieldHandler; }
			internal set { editFieldHandler = value; }
		}

		public IEditFieldHandlerDatabaseInterface DataHandler
		{
			get { return dataHandler; }
		}

		public IEditFieldData Data
		{
			get { return data; }
			set { data = value; }
		}

		public EditFieldInfo()
		{
		}

		public EditFieldInfo(long editFieldID, string sectionName, string fieldName, int rank, IEditFieldHandler editFieldHandler, IEditFieldHandlerDatabaseInterface dataHandler)
		{
			this.fieldName = fieldName;
			this.sectionName = sectionName;
			this.editFieldID = editFieldID;
			this.editFieldHandler = editFieldHandler;
			this.dataHandler = dataHandler;
			this.rank = rank;
		}

		public bool Read(IDataReader reader)
		{
			editFieldID = (long)reader["EditFieldID"];
			fieldName = (string)reader["FieldName"];
			sectionName = (string)reader["SectionName"];
			rank = Convert.ToInt32(reader["Rank"]);
			IEditFieldObjectCreator c = ContentManager.GetEditFieldObjectCreator(reader["EditFieldTypeIdentifier"].ToString());
			if (c == null)
				return false;
			editFieldHandler = c.CreateHandler();
			dataHandler = c.CreateDatabaseInterface();
			return true;
		}

		public int CompareTo(object obj)
		{
			EditFieldInfo node = obj as EditFieldInfo;
			if (obj == null) return 1;
			int n = sectionName.CompareTo(node.SectionName);
			if (n != 0) return n;
			n = fieldName.CompareTo(node.FieldName);
			if (n != 0) return n;
			return rank.CompareTo(node.rank);
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "render_admin":
				case "render_content":
				case "field_name":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "render_admin": return Handler.RenderAdminField(data);
				case "render_content": return Handler.RenderContent(data);
				case "field_name": return fieldName;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property for this field.", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
