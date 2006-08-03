using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

using Sprocket.Web;

namespace Sprocket.Web.CMS.Pages
{
	public class ListRegistry
	{
		private XmlDocument listsDoc = null;
		private Dictionary<string, ListDefinition> lists = null;
		private DateTime fileDate = DateTime.MinValue;
		private string listsDocPath;

		private ListRegistry()
		{
			Init();
		}

		public static void Reload()
		{
			instance = new ListRegistry();
		}

		private void Init()
		{
			listsDocPath = WebUtility.MapPath("resources/definitions/lists.xml");
			listsDoc = new XmlDocument();
			listsDoc.Load(listsDocPath);
			lists = new Dictionary<string, ListDefinition>();
			fileDate = File.GetLastWriteTime(listsDocPath);
		}

		public ListDefinition this[string name]
		{
			get
			{
				if (File.GetLastWriteTime(listsDocPath) > fileDate)
					Init();

				if (lists.ContainsKey(name))
					return lists[name];
				
				XmlNode node = listsDoc.SelectSingleNode("/Lists/List[@Name='" + name + "']");
				if (node == null)
				{
					lists.Add(name, null);
					return null;
				}
				ListDefinition list = new ListDefinition((XmlElement)node);
				lists.Add(name, list);
				return list;
			}
		}

		#region Instance
		private static ListRegistry instance = null;
		public static ListRegistry Lists
		{
			get
			{
				if (instance == null)
					instance = new ListRegistry();
				return instance;
			}
		}
		#endregion
	}
}
