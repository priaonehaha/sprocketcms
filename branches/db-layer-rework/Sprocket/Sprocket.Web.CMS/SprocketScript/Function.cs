using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.SprocketScript
{
	public interface IFunction
	{
		
	}

	public interface IFunctionCreator
	{
		string Keyword { get; }
		IFunction Create();
	}
}
