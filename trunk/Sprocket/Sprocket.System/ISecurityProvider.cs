using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Security
{
	public interface ISecurityProvider
	{
		bool IsValidLogin(string username, string passwordHash);
	}
}
