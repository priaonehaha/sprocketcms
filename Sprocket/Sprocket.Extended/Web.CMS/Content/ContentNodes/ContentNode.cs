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
	/// A content node is an individual piece of editable (or maybe non-editable) data inside a page admin section.
	/// A page admin section has a label, a help hint and zero or more content nodes that can be edited. Some page
	/// admin sections allow you to add extra content nodes of your choosing, although this is defined in definitions.xml.
	/// </summary>
	public class ContentNode : IComparable
	{
		private IContentNodeType contentNodeType;
		private long contentNodeID;
		private int rank;
		private string fieldName;

		public string FieldName
		{
			get { return fieldName; }
		}

		public int Rank
		{
			get { return rank; }
		}

		public long ID
		{
			get { return contentNodeID; }
		}

		public IContentNodeType Type
		{
			get { return contentNodeType; }
		}

		public ContentNode(long contentNodeID, string fieldName, int rank, IContentNodeType contentNodeType)
		{
			this.fieldName = fieldName;
			this.contentNodeID = contentNodeID;
			this.contentNodeType = contentNodeType;
			this.rank = rank;
		}

		public ContentNode(IDataReader reader)
			: this((long)reader["ContentNodeID"], (string)reader["Name"], Convert.ToInt32(reader["Rank"]),
				ContentManager.GetNodeType(reader["NodeTypeIdentifier"].ToString()))
		{
		}

		public int CompareTo(object obj)
		{
			ContentNode node = obj as ContentNode;
			if (obj == null) return 1;
			int n = fieldName.CompareTo(node.FieldName);
			if (n != 0) return n;
			return rank.CompareTo(node.rank);
		}
	}
}
