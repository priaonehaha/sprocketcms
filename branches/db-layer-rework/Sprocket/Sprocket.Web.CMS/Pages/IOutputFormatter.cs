using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public interface IOutputFormatter
	{
		string Format(string input, XmlElement xmlSettings);
	}
}
