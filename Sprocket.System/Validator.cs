using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sprocket.Utility
{
	public static partial class Utilities
	{
		public static class Validator
		{
			public static bool IsEmailAddress(string email)
			{
				return Regex.IsMatch(email, @"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.IgnoreCase);
			}
		}
	}
}
