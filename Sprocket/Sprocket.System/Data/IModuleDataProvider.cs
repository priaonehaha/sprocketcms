using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Sprocket.Data
{
	public interface IModuleDataProvider
	{
		/// <summary>
		/// Return a type implementing IDatabaseHandler
		/// </summary>
		Type DatabaseHandlerType { get; }
		/// <summary>
		/// Initialise schema, stored procedures, etc. Use result.SetFailed if a problem occurs.
		/// </summary>
		/// <param name="result"></param>
		void Initialise(Result result);
	}
}
