using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace Sprocket.Web.CMS.Pages
{
	public class PageRegistry
	{
		private XmlDocument pagesDoc = null;
		private List<PageEntry> pages = null;
		private Dictionary<string, PageEntry> requestPaths = null;
		private Dictionary<string, PageEntry> pageCodes = null;
		private Dictionary<Guid, PageEntry> pageIDs = null;
		private Dictionary<string, PageEntry> contentFiles = null;
		private DateTime fileDate = DateTime.MinValue;
		private string pagesDocPath;

		private PageRegistry()
		{
			Init();
		}

		public static void Reload()
		{
			instance = new PageRegistry();
		}

		private void Init()
		{
			pages = new List<PageEntry>();
			requestPaths = new Dictionary<string, PageEntry>();
			pageCodes = new Dictionary<string, PageEntry>();
			pageIDs = new Dictionary<Guid, PageEntry>();
			contentFiles = new Dictionary<string, PageEntry>();
			pagesDocPath = WebUtility.MapPath("resources/definitions/pages.xml");
			pagesDoc = new XmlDocument();
			if (!File.Exists(pagesDocPath))
				return;
			pagesDoc.Load(pagesDocPath);
			fileDate = File.GetLastWriteTime(pagesDocPath);
			foreach (XmlElement node in pagesDoc.DocumentElement.ChildNodes)
				LoadEntry(node, null);
		}

		private PageEntry LoadEntry(XmlElement xml, PageEntry parent)
		{
			PageEntry pageEntry = new PageEntry();
			pageEntry.Parent = parent;
			pageEntry.TemplateName = xml.GetAttribute("Template");
			pageEntry.ContentFile = xml.GetAttribute("ContentFile");
			if (xml.HasAttribute("PageID"))
			{
				pageEntry.PageID = new Guid(xml.GetAttribute("PageID"));
				pageIDs[pageEntry.PageID.Value] = pageEntry;
			}
			if (xml.HasAttribute("Path"))
			{
				pageEntry.Path = xml.GetAttribute("Path").ToLower();
				requestPaths[pageEntry.Path] = pageEntry;
			}
			if (xml.HasAttribute("Code"))
			{
				pageEntry.PageCode = xml.GetAttribute("Code");
				pageCodes[pageEntry.PageCode] = pageEntry;
			}
			if (xml.HasAttribute("ContentFile"))
			{
				pageEntry.ContentFile = xml.GetAttribute("ContentFile");
				contentFiles[pageEntry.ContentFile] = pageEntry;
			}
			//else throw new Exception("Sprocket.Web.CMS.Pages.PageRegistry: A Page entry has been detected that is missing a PageCode attribute");

			foreach (XmlElement node in xml.ChildNodes)
				pageEntry.Pages.Add(LoadEntry(node, pageEntry));

			pages.Add(pageEntry);
			return pageEntry;
		}

		public static void CheckDate()
		{
			if (File.GetLastWriteTime(Pages.pagesDocPath) > Pages.fileDate)
				Pages.Init();
		}

		public PageEntry FromPath(string sprocketPath)
		{
			if(requestPaths.ContainsKey(sprocketPath))
				return requestPaths[sprocketPath];
			return null;
		}

		public PageEntry FromPageCode(string pageCode)
		{
			if (pageCodes.ContainsKey(pageCode))
				return pageCodes[pageCode];
			return null;
		}

		public PageEntry FromContentFile(string contentFile)
		{
			if (contentFiles.ContainsKey(contentFile))
				return contentFiles[contentFile];
			return null;
		}

		public PageEntry FromPageID(Guid pageID)
		{
			if(pageIDs.ContainsKey(pageID))
				return pageIDs[pageID];
			return null;
		}

		public List<PageEntry> SelectPages(string xpath)
		{
			List<PageEntry> list = new List<PageEntry>();
			XmlNodeList nodes = pagesDoc.SelectNodes(xpath);
			foreach(XmlNode node in nodes)
			{
				if (node.NodeType != XmlNodeType.Element)
					continue;

				XmlElement e = (XmlElement)node;
				PageEntry page = null;
				if (e.HasAttribute("PageID"))
					page = FromPageID(new Guid(e.GetAttribute("PageID")));
				else if (e.HasAttribute("PageCode"))
					page = FromPageCode(e.GetAttribute("PageCode"));
				else if (e.HasAttribute("Path"))
					page = FromPath(e.GetAttribute("Path"));
				else if (e.HasAttribute("ContentFile"))
					page = FromContentFile(e.GetAttribute("ContentFile"));

				if (page != null)
					list.Add(page);
			}
			return list;
		}

		public PageEntry SelectSinglePage(string xpath)
		{
			XmlNode node = pagesDoc.SelectSingleNode(xpath);
			if (node == null)
				return null;
			if (!(node is XmlElement))
				return null;
			XmlElement e = (XmlElement)node;
			if(e.HasAttribute("PageID"))
				return FromPageID(new Guid(e.GetAttribute("PageID")));
			if(e.HasAttribute("PageCode"))
				return FromPageCode(e.GetAttribute("PageCode"));
			return FromPath(e.GetAttribute("Path"));
		}

		#region Instance
		private static PageRegistry instance = null;
		public static PageRegistry Pages
		{
			get
			{
				if (instance == null)
					instance = new PageRegistry();
				return instance;
			}
		}
		#endregion

		public static void UpdateValues()
		{
			/*
			string path = WebUtility.MapPath("resources/definitions/pages.xml");
			if (!File.Exists(path)) return;

			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			foreach (XmlNode node in doc.DocumentElement.SelectNodes("//Page"))
			{
				if (node is XmlElement)
				{
					XmlElement e = (XmlElement)node;
					if (!e.HasAttribute("PageID"))
						e.SetAttribute("PageID", Guid.NewGuid().ToString());
				}
			}
			doc.Save(path);
			 */
		}
	}
}
