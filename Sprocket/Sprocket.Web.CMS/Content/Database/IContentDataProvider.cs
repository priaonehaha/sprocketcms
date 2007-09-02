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
	}
}
