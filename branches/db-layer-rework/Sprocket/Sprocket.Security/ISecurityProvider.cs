using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Security
{
	public interface ISecurityProvider
	{
		public ISecurityProviderDataProxy DataProxy { get; }

	}
}
