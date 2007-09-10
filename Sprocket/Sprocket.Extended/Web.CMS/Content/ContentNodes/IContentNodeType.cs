using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Content
{
	public interface IContentNodeType
	{
		void Initialise(XmlElement xml);
		string RenderContent();
		string RenderAdminField();
		Result ReadAdminField();
		bool IsContentDifferent(IContentNodeType previousNodeVersion);
	}

	public interface IContentNodeDatabaseInterface
	{
		void LoadNodeData(List<ContentNode> nodes);
	}

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

	public interface IContentNodeTypeCreator
	{
		string Identifier { get; }
		IContentNodeType Create();
		IContentNodeDatabaseInterface CreateDatabaseInterface();
	}

	public class TextBoxContentNode : IContentNodeType
	{
		public void Initialise(XmlElement xml)
		{

		}

		public string RenderContent()
		{
			return "render content";
		}

		public string RenderAdminField()
		{
			return "render admin";
		}

		public Result ReadAdminField()
		{
			return new Result();
		}

		public bool IsContentDifferent(IContentNodeType previousNodeVersion)
		{
			return false;
		}

		public class TextBoxContentNodeCreator : IContentNodeTypeCreator
		{
			public string Identifier
			{
				get { return "TextBox"; }
			}

			public IContentNodeType Create()
			{
				return new TextBoxContentNode();
			}

			public IContentNodeDatabaseInterface CreateDatabaseInterface()
			{
				return new TextBoxNodeDatabaseInterface();
			}
		}

		public class TextBoxNodeDatabaseInterface : IContentNodeDatabaseInterface
		{
			public void LoadNodeData(List<ContentNode> nodes)
			{
			}
		}
	}
}
