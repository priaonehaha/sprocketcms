using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.Pages
{
	public class Template
	{
		private PlaceHolder[] placeHolders = null;
		private string text = null;

		public Template(string source)
		{
			text = source;
		}

		public PlaceHolder[] PlaceHolders
		{
			get
			{
				if (placeHolders == null)
					placeHolders = PlaceHolder.Extract(text);
				return placeHolders;
			}
		}

		public string Text
		{
			get { return text; }
		}
	}
}
