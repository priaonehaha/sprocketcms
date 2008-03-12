using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;
using System.Transactions;

using Sprocket.Web;

namespace Sprocket.Data
{
	public class SQLiteStoredProcedures
	{
		Dictionary<string, string> procs = new Dictionary<string,string>();
		public SQLiteStoredProcedures(string text)
		{
			string name = null;
			StringBuilder sql = new StringBuilder();
			StringReader sr = new StringReader(text);
			string line = sr.ReadLine();
			while (line != null)
			{
				line = line.Trim();
				if (line.Length > 0)
				{
					if (line.StartsWith("--#"))
					{
						if (name != null)
						{
							if (sql.Length > 0)
							{
								if (procs.ContainsKey(name))
									throw new Exception("There's already a procedure named " + name);
								procs.Add(name, sql.ToString());
								sql = new StringBuilder();
							}
						}
						name = line.Substring(3).Trim();
					}
					else
						sql.AppendLine(line);
				}
				line = sr.ReadLine();
			}
			if (sql.Length > 0)
			{
				if (procs.ContainsKey(name))
					throw new Exception("There's already a procedure named " + name);
				procs.Add(name, sql.ToString());
			}
		}

		public string this[string name]
		{
			get
			{
				string s;
				if (!procs.TryGetValue(name, out s))
					throw new Exception("There is no procedure named \"" + name + "\".");
				return s;
			}
		}
	}
}
