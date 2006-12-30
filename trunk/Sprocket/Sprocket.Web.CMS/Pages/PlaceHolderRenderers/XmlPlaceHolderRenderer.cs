using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class XmlPlaceHolderRenderer : IPlaceHolderRenderer
	{
		public virtual string Render(PlaceHolder placeHolder, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			return Render(placeHolder.Expression, pageEntry, content, placeHolderStack, out containsCacheableContent);
		}

		public string Render(string xpath, PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			string text = "";
			XmlNodeList nodes;
			try
			{
				nodes = content.SelectNodes(xpath);
			}
			catch
			{
				containsCacheableContent = false;
				return "[Cannot render placeholder; invalid XPath expression]";
			}
			if (nodes.Count == 0)
			{
				containsCacheableContent = false;
				return null;
			}
			bool cacheable = true;
			foreach (XmlElement node in nodes)
			{
				if (node != null)
				{
					if (node.HasAttribute("Embed"))
					{
						PageEntry embedPageEntry = PageRegistry.Pages.FromPageCode(node.GetAttribute("Embed"));
						if (embedPageEntry == null)
							text += "[Embed failed. Page code \"" + node.GetAttribute("Embed") + "\" not found]";
						else
						{
							bool returnValueCached;
							string embed = embedPageEntry.Render(placeHolderStack, out returnValueCached);
							cacheable = cacheable && returnValueCached;
							if (embed == null)
								text += "[Content not found for embedded page " + pageEntry.PageCode + "]";
							else
								text += embed;
						}
					}
					else
					{
						if (node.FirstChild != null)
						{
							bool cache;
							string str = node.FirstChild.Value;
							foreach (PlaceHolder ph in PlaceHolder.Extract(str))
							{
								str = str.Replace(ph.RawText, ph.Render(pageEntry, content, placeHolderStack, out cache));
								cacheable = cacheable && cache;
							}
							text += str;
						}
					}
				}
				else
					text += "[Content not found for node " + xpath + "]";
			}
			containsCacheableContent = cacheable;
			return text;
		}
	}
}
