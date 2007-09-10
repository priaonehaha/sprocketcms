using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Content
{
	public interface IContentDataProvider : IModuleDataProvider
	{
		Result Store(RevisionInformation revisionInformation);
		Result Store(Page page);

		RevisionInformation SelectRevisionInformation(long revisionID);
		Page SelectPage(long pageID);
		Page SelectPageByPageCode(string pageCode);
		List<Page> ListPages();
		Dictionary<string, List<ContentNode>> ListContentNodesForPage(long pageRevisionID);
	}
}
