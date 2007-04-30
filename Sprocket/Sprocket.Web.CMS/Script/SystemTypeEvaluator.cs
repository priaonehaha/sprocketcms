using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.Script
{
	internal static class SystemTypeEvaluator
	{
		public static object EvaluateProperty(object o, string propertyName, Token propertyNameToken)
		{
			if (o is string)
			{
				switch (propertyName)
				{
					case "length": return ((string)o).Length;
				}
			}
			throw new InstructionExecutionException("I can't evaluate this property because the value that it pertains to does not have script support for this property name. (Underlying type: " + o.GetType().FullName + ")", propertyNameToken);
		}
	}
}
