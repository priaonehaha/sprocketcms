using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sprocket.Web;

namespace Sprocket.Web.CMS.Pages
{
	public class PageEntry
	{
		#region Fields and Properties

		private string path = "", contentFile = "", template = "", pageCode = "";
		private Guid? pageID = null;
		private Guid internalID = Guid.NewGuid();
		private PageEntry parent = null;
		private List<PageEntry> pages = new List<PageEntry>();

		public List<PageEntry> Pages
		{
			get { return pages; }
			set { pages = value; }
		}

		public string PageCode
		{
			get { return pageCode; }
			set { pageCode = value; }
		}

		public Guid? PageID
		{
			get { return pageID; }
			set { pageID = value; }
		}

		public Guid InternalID
		{
			get { return internalID; }
			set { internalID = value; }
		}

		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		public string ContentFile
		{
			get { return contentFile; }
			set { contentFile = value; }
		}

		public string TemplateName
		{
			get { return template; }
			set { template = value; }
		}

		public Template Template
		{
			get { return TemplateRegistry.Templates[template]; }
		}

		public PageEntry Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		#endregion

		public XmlDocument LoadContentDocument()
		{
			return PageRequestHandler.Instance.GetXmlDocument("resources/content/" + contentFile);
		}

		public string Render()
		{
			bool whocares;
			return Render(new Stack<string>(), out whocares);
		}

		public string Render(Stack<string> placeHolderStack, out bool pageOutputCached)
		{
			string cachePath;
			if (pageCode != null && pageCode != "")
				cachePath = "$pagecode[" + PageCode + "]";
			else if (pageID != null)
				cachePath = "$pageID[" + PageID + "]";
			else
				cachePath = path;

			string output = null;

			if (ContentCache.IsContentCached(cachePath))
				output = ContentCache.ReadCache(cachePath);

			if (output != null)
			{
				pageOutputCached = true;
				return output;
			}

			XmlDocument content = new XmlDocument();
			if (contentFile.Length > 0)
			{
				content = LoadContentDocument();
				if (content == null)
				{
					pageOutputCached = false;
					return "[Unable to load the content file " + contentFile + "]";
				}
			}

			bool cacheable = true;
			output = Template.Text;
			foreach (PlaceHolder placeholder in Template.PlaceHolders)
			{
				bool returnValueCacheable;
				output = output.Replace(placeholder.RawText, placeholder.Render(this, content, placeHolderStack, out returnValueCacheable));
				cacheable = cacheable && returnValueCacheable;
			}

			pageOutputCached = cacheable;
			if (cacheable)
				ContentCache.CacheContent(cachePath, output);

			return output;
		}
	}
}
