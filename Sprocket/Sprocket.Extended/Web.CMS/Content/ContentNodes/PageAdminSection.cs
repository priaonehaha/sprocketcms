using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Content
{
	/// <summary>
	/// Represents a mapping of a page admin section definition to a list of nodes prepared/retrieved
	/// for editing within that section.
	/// </summary>
	public class PageAdminSection
	{
		private PageAdminSectionDefinition def;
		public PageAdminSectionDefinition SectionDefinition
		{
			get { return def; }
			internal set { def = value; }
		}

		private List<ContentNode> nodeList;
		public List<ContentNode> NodeList
		{
			get { return nodeList; }
			internal set { nodeList = value; }
		}
	}
}
