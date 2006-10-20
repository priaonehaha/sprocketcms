using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class ListDefinition
	{
		private string name = "", selectExpr = null;
		private Template itemTemplate = null, alternatingItemTemplate = null, dividerTemplate = null;
		private Dictionary<int, Template> singleItemTemplate = null;
		private bool isReverseOrder = false;

		public string Render(Stack<string> placeHolderStack, out bool isCacheable)
		{
			isCacheable = true;

			if (itemTemplate == null)
				return "[Cannot render list \"" + name + "\". No ItemTemplate specified]";

			if (selectExpr == null || selectExpr == "")
				return "[Cannot render list \"" + name + "\". No page selection expression has been specified]";

			List<PageEntry> pages = PageRegistry.Pages.SelectPages(selectExpr);
			bool alt = false;
			StringBuilder output = new StringBuilder();
			bool cacheable = true;
			int startx, endx, incrementx;
			if (isReverseOrder)
			{
				startx = pages.Count - 1;
				endx = 0;
				incrementx = -1;
			}
			else
			{
				startx = 0;
				endx = pages.Count - 1;
				incrementx = 1;
			}
			int c = 1;
			for(int x = startx; x <= pages.Count-1 && x >= 0; x += incrementx)
			{
				PageEntry page = pages[x];
				Template t = null;
				if (singleItemTemplate != null)
					if (singleItemTemplate.ContainsKey(c))
						t = singleItemTemplate[c];
				if(t == null)
					t = alt && alternatingItemTemplate != null ? alternatingItemTemplate : itemTemplate;
				string canvas = t.Text;
				XmlDocument content = PageRequestHandler.Instance.GetXmlDocument("resources/content/" + page.ContentFile);
				if(content == null)
				{
					output.Append("[Unable to load content document " + page.ContentFile + "]");
					continue;
				}
				bool containsCacheableContent;
				foreach (PlaceHolder ph in t.PlaceHolders)
				{
					canvas = canvas.Replace(ph.RawText, ph.Render(page, content, placeHolderStack, out containsCacheableContent));
					cacheable = cacheable && containsCacheableContent;
				}
				if (output.Length > 0 && dividerTemplate != null)
					output.Append(dividerTemplate.Text);
				output.Append(canvas); 
				alt = !alt;
				c++;
			}
			isCacheable = cacheable;
			return output.ToString();
		}

		public ListDefinition(XmlElement xmlDefinition)
		{
			name = xmlDefinition.GetAttribute("Name");
			isReverseOrder = xmlDefinition.GetAttribute("Order").ToLower() == "reverse";

			XmlNode node = xmlDefinition.SelectSingleNode("./ItemTemplate[string-length(@Name) > 0]");
			if (node != null)
			{
				itemTemplate = TemplateRegistry.Templates[((XmlElement)node).GetAttribute("Name")];
				node = null;
			}

			node = xmlDefinition.SelectSingleNode("./AlternatingItemTemplate[string-length(@Name) > 0]");
			if (node != null) 
			{
				alternatingItemTemplate = TemplateRegistry.Templates[((XmlElement)node).GetAttribute("Name")];
				node = null;
			}

			node = xmlDefinition.SelectSingleNode("./DividerTemplate[string-length(@Name) > 0]");
			if (node != null)
			{
				alternatingItemTemplate = TemplateRegistry.Templates[((XmlElement)node).GetAttribute("Name")];
				node = null;
			}

			XmlNodeList nodes = xmlDefinition.SelectNodes("./SingleItemTemplate[string-length(@Name) > 0 and @Index > 0]");
			if (nodes.Count > 0)
			{
				singleItemTemplate = new Dictionary<int, Template>();
				foreach (XmlNode snode in nodes)
				{
					Template t = TemplateRegistry.Templates[((XmlElement)snode).GetAttribute("Name")];
					int n = int.Parse(((XmlElement)snode).GetAttribute("Index"));
					singleItemTemplate[n] = t;
				}
			}

			node = xmlDefinition.SelectSingleNode("./PageSelectExpression[1]");
			if (node != null)
				if (node.FirstChild != null)
					if (node.FirstChild.NodeType == XmlNodeType.Text || node.FirstChild.NodeType == XmlNodeType.CDATA)
						selectExpr = node.FirstChild.Value.Trim();
			if (selectExpr == null && xmlDefinition.HasAttribute("ParentPageCode"))
				selectExpr = "//*[@Name='" + xmlDefinition.GetAttribute("ParentPageCode") + "']/Page";
		}
	}
}
