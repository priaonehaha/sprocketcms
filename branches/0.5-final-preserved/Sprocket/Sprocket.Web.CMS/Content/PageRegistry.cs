using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using Sprocket.Utility;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class PageRegistry
	{
		private List<PageEntry> pages = null;
		private Dictionary<string, PageEntry> requestPaths = null;
		private Dictionary<string, PageEntry> pageCodes = null;
		private Dictionary<string, PageEntry> contentFiles = null;
		private List<PageEntry> flexiblePaths = null;
		private DateTime fileDate = DateTime.MinValue;

		public PageRegistry()
		{
			Init();
		}

		private void Init()
		{
			pages = new List<PageEntry>();
			requestPaths = new Dictionary<string, PageEntry>();
			pageCodes = new Dictionary<string, PageEntry>();
			contentFiles = new Dictionary<string, PageEntry>();
			flexiblePaths = new List<PageEntry>();
			foreach (XmlNode node in ContentManager.DefinitionsXml.SelectNodes("/Definitions/Pages/*"))
				if (node is XmlElement)
					LoadEntry((XmlElement)node, null);
		}

		private PageEntry LoadEntry(XmlElement xml, PageEntry parent)
		{
			PageEntry pageEntry = new PageEntry();
			pageEntry.Parent = parent;
			pageEntry.TemplateName = xml.GetAttribute("Template");
			pageEntry.ContentFile = xml.GetAttribute("ContentFile");
			if (xml.HasAttribute("ContentType"))
				pageEntry.ContentType = xml.GetAttribute("ContentType");

			if (xml.HasAttribute("Path"))
			{
				pageEntry.Path = xml.GetAttribute("Path").ToLower();
				requestPaths[pageEntry.Path] = pageEntry;
				if (xml.HasAttribute("HandleSubPaths"))
				{
					pageEntry.HandleSubPaths = StringUtilities.BoolFromString(xml.GetAttribute("HandleSubPaths"));
					flexiblePaths.Add(pageEntry);
				}
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

			foreach (XmlElement node in xml.ChildNodes)
				pageEntry.Pages.Add(LoadEntry(node, pageEntry));

			pages.Add(pageEntry);
			return pageEntry;
		}

		public PageEntry FromPath(string sprocketPath)
		{
			if (requestPaths.ContainsKey(sprocketPath))
				return requestPaths[sprocketPath];
			foreach (PageEntry page in flexiblePaths)
			{
				if (!File.Exists(SprocketPath.Physical))
					if (page.Path == "")
						return page;
					else if (SprocketPath.IsCurrentPathDescendentOf(page.Path))
						return page;
			}
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

		public List<PageEntry> SelectPages(string xpath)
		{
			List<PageEntry> list = new List<PageEntry>();
			XmlNodeList nodes = ContentManager.DefinitionsXml.SelectSingleNode("/Definitions/Pages").SelectNodes(xpath);
			foreach (XmlNode node in nodes)
			{
				if (node.NodeType != XmlNodeType.Element)
					continue;

				XmlElement e = (XmlElement)node;
				PageEntry page = null;
				if (e.HasAttribute("PageCode"))
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
			XmlNode node = ContentManager.DefinitionsXml.SelectSingleNode("/Definitions/Pages").SelectSingleNode(xpath);
			if (node == null)
				return null;
			if (!(node is XmlElement))
				return null;
			XmlElement e = (XmlElement)node;
			if (e.HasAttribute("PageCode"))
				return FromPageCode(e.GetAttribute("PageCode"));
			return FromPath(e.GetAttribute("Path"));
		}
	}
}
