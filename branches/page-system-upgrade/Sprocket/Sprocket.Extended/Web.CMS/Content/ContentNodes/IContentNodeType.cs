using System;
using System.Collections.Generic;
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

	public interface IContentNodeTypeCreator
	{
		string Identifier { get; }
		IContentNodeType Create();
	}

	public class TextBoxContentNode : IContentNodeType
	{
		public void Initialise(XmlElement xml)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public string RenderContent()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public string RenderAdminField()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Result ReadAdminField()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsContentDifferent(IContentNodeType previousNodeVersion)
		{
			throw new Exception("The method or operation is not implemented.");
		}
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
	}
}
