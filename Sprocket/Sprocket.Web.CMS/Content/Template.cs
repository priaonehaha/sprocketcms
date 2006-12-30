using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class Template
	{
		private Dictionary<string, DateTime> fileTimes;
		public Dictionary<string, DateTime> FileTimes
		{
			get { return fileTimes; }
		}

		SprocketScript script;
		public SprocketScript Script
		{
			get { return script; }
		}

		public Template(SprocketScript script, Dictionary<string, DateTime> fileTimes)
		{
			this.fileTimes = fileTimes;
			this.script = script;
		}

		public bool IsOutOfDate
		{
			get
			{
				foreach (KeyValuePair<string, DateTime> kvp in fileTimes)
				{
					FileInfo file = new FileInfo(kvp.Key);
					if (!file.Exists)
						return true;
					if (file.LastWriteTime != kvp.Value)
						return true;
				}
				return false;
			}
		}
	}
}
