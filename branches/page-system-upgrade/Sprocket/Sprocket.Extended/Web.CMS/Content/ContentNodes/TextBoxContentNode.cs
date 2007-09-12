using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Content
{
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

		public string TypeName
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}
	}
}
