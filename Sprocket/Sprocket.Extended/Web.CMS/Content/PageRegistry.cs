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
		private List<PageEntry> flexiblePaths = null;
		private DateTime fileDate = DateTime.MinValue;
		private string prefixAllPathsWith = "";
		private TemplateRegistry templates = null;

		public PageRegistry(TemplateRegistry templates, string prefixAllPathsWith)
		{
			this.templates = templates;
			this.prefixAllPathsWith = prefixAllPathsWith;
		}

		public PageRegistry(TemplateRegistry templates, XmlElement pagesRootNode, string prefixAllPathsWith)
		{
			this.templates = templates;
			this.prefixAllPathsWith = prefixAllPathsWith;
			Load(pagesRootNode);
		}

		public PageRegistry(TemplateRegistry templates, XmlElement pagesRootNode)
		{
			this.templates = templates;
			Load(pagesRootNode);
		}

		public PageRegistry(TemplateRegistry templates)
		{
			this.templates = templates;
		}

		public void Load(XmlElement pagesRootNode)
		{
			if (pages == null)
			{
				pages = new List<PageEntry>();
				requestPaths = new Dictionary<string, PageEntry>();
				pageCodes = new Dictionary<string, PageEntry>();
				flexiblePaths = new List<PageEntry>();
			}
			foreach (XmlNode node in pagesRootNode.SelectNodes("*"))
				if (node is XmlElement)
					LoadEntry((XmlElement)node, null);
		}

		private PageEntry LoadEntry(XmlElement xml, PageEntry parent)
		{
			PageEntry pageEntry = new PageEntry(templates);
			pageEntry.Parent = parent;
			pageEntry.TemplateName = xml.GetAttribute("Template");
			if (xml.HasAttribute("ContentType"))
				pageEntry.ContentType = xml.GetAttribute("ContentType");

			if (xml.HasAttribute("Path"))
			{
				pageEntry.Path = xml.GetAttribute("Path").ToLower();
				if (prefixAllPathsWith != null && prefixAllPathsWith != String.Empty)
				{
					if (pageEntry.Path.Length > 0)
						pageEntry.Path = prefixAllPathsWith + "/" + pageEntry.Path;
					else
						pageEntry.Path = prefixAllPathsWith;
				}
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
	}
}
