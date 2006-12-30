using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sprocket.Web.CMS.Pages
{
	public class PlaceHolder
	{
		#region Static Methods

		public static PlaceHolder[] Extract(string source)
		{
			MatchCollection matches = Regex.Matches(source, GetRegexPattern(), RegexOptions.IgnoreCase);
			PlaceHolder[] placeholders = new PlaceHolder[matches.Count];
			for (int i = 0; i < matches.Count; i++)
				placeholders[i] = new PlaceHolder(matches[i]);
			return placeholders;
		}

		public static string GetRegexPattern()
		{
			return GetRegexPattern(null);
		}

		public static string GetRegexPattern(string specificName)
		{
			string nameExpr;
			if (specificName == null)
				nameExpr = "(?::\"(?<Name>[^\"]*)\")?";
			else
				nameExpr = ":\"" + specificName + "\"";
			return @"\{"
				 + @"(?<Prefix>[a-zA-Z0-9_\-]+)"
				 + nameExpr
				 + @"(?::(?<Format>[a-zA-Z0-9_\-]+)(?=:))?"
				 + @"(?::(?<Value>[^\}]+))?"
				 + @"\}";
		}

		#endregion

		private PlaceHolder(Match match)
		{
			rawText = match.Groups[0].Value;
			name = match.Groups["Name"].Value;
			prefix = match.Groups["Prefix"].Value;
			expression = match.Groups["Value"].Value;
			
			string formatName = match.Groups["Format"].Value;
			if (formatName.Length > 0)
				outputFormat = OutputFormatRegistry.Formats[formatName];
			string type = outputFormat == null ? "" : outputFormat.GetAttribute("OutputType");
			if (PageRequestHandler.Instance.OutputFormatters.ContainsKey(type))
				outputFormatter = PageRequestHandler.Instance.OutputFormatters[type];
			else
				outputFormatter = new MissingOutputFormatter();

			string lprefix = prefix.ToLower();
			if (PageRequestHandler.Instance.PlaceHolderRenderers.ContainsKey(lprefix))
				this.renderer = PageRequestHandler.Instance.PlaceHolderRenderers[lprefix];
			else
				this.renderer = new UnknownPlaceHolderRenderer();
		}

		private string name = null;
		private string prefix = null;
		private XmlElement outputFormat = null;
		private IOutputFormatter outputFormatter = null;
		private string expression = null;
		private string rawText = null;
		private IPlaceHolderRenderer renderer = null;

		public string RawText
		{
			get { return rawText; }
		}

		public string Name
		{
			get { return name; }
		}

		public string Prefix
		{
			get { return prefix; }
		}

		public string Expression
		{
			get { return this.expression; }
		}

		public string Render(PageEntry pageEntry, XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			string phcode = pageEntry.InternalID.ToString() + ";" + rawText;
			if (placeHolderStack.Contains(phcode))
			{
				containsCacheableContent = false;
				return "[Unable to render placeholder; circular dependency detected]";
			}

			placeHolderStack.Push(phcode);

			string output;
			if (this.renderer == null)
			{
				containsCacheableContent = false;
				output = "[No renderer exists for placeholders of type \"" + prefix + "\"]";
			}
			else
			{
				if (placeHolderStack == null)
					placeHolderStack = new Stack<string>();
				output = renderer.Render(this, pageEntry, content, placeHolderStack, out containsCacheableContent);
				if (output == null)
					output = RawText;
				else
					output = outputFormatter.Format(output, outputFormat);
			}

			placeHolderStack.Pop();
			return output;
		}
	}
}
