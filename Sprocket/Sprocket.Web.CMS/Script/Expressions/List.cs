using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	public class ListExpression : IExpression
	{
		IList list = null;

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return list;
		}

		public ListExpression(IList list)
		{
			this.list = list;
		}

		public override string ToString()
		{
			return "{ListExpression}";
		}
	}
}
