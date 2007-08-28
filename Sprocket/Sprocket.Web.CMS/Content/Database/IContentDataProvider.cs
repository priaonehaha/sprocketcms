using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.Content
{
	public interface IContentDataProvider
	{
		Result Initialise();
		Result Store(RevisionInformation revisionInformation);
	}
}
