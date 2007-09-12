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
		string TypeName { get; }
		string RenderContent();
		string RenderAdminField();
		Result ReadAdminField();
		bool IsContentDifferent(IContentNodeType previousNodeVersion);
	}

	public interface IContentNodeDatabaseInterface
	{
		void LoadNodeData(List<ContentNode> nodes);
	}

	public interface IContentNodeTypeCreator
	{
		string Identifier { get; }
		IContentNodeType Create();
		IContentNodeDatabaseInterface CreateDatabaseInterface();
	}
}
