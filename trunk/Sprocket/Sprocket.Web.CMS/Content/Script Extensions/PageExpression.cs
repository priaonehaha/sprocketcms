using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;
using System.Xml;

namespace Sprocket.Web.CMS.Content.Expressions
{
	public class PageExpression : IExpression
	{
		private delegate object RenderHandler(ExecutionState state);
		private RenderHandler RenderValue = null;
		private string xpathExpr = null;
		private IExpression pageCodeExpr = null;
		private Token pageToken;
		private Token xpathToken;

		private static Dictionary<string, XmlDocument> xmlDocumentTempCache = new Dictionary<string, XmlDocument>();

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			pageToken = tokens[index - 1];
			Token token = tokens[index];
			if (token.TokenType == TokenType.Symbolic)
			{
				// if the token is a colon, then we're extracting specific information relating to the page
				if (token.Value == ":")
				{
					index++;
					token = tokens[index++];
					// if it's a word, see what the word is
					if (token.TokenType == TokenType.Word)
					{
						switch (token.Value)
						{
							case "path": // get the sprocket path for the page
								RenderValue = RenderPagePath;
								break;

							//default:
							//    throw new TokenParserException("I can't get the page info requested here because \"" + token.Value + "\" isn't an attribute of the specified page.", token);
						}
					} // otherwise if it's a string, it's an xpath expression
					else if(token.TokenType == TokenType.StringLiteral && !token.IsNonScriptText)
					{
						xpathExpr = token.Value; //TokenParser.BuildExpression(tokens, ref index, precedenceStack);
						xpathToken = token;
						RenderValue = RenderPageXML;
					}
				}
			}

			token = tokens[index];
			if((token.TokenType == TokenType.StringLiteral && !token.IsNonScriptText) || token.TokenType == TokenType.GroupStart)
				pageCodeExpr = TokenParser.BuildExpression(tokens, ref index, precedenceStack, true);
			
			if(RenderValue == null)
				RenderValue = RenderPage;
		}

		public object Evaluate(ExecutionState state)
		{
			return RenderValue(state);
		}

		bool popStack;
		private PageEntry GetPage(ExecutionState state)
		{
			if (pageCodeExpr == null)
			{
				popStack = false;
				if (ContentManager.PageStack.Count == 0)
					throw new InstructionExecutionException("The current page was requested but somebody forgot to note down what the current page is!", pageToken);
				else
					return ContentManager.PageStack.Peek();
			}
			popStack = true;
			PageEntry page = ContentManager.Pages.FromPageCode(pageCodeExpr.Evaluate(state).ToString());
			ContentManager.PageStack.Push(page);
			if (page == null)
				throw new InstructionExecutionException("I can't get information about the page because the one specified doesn't seem to exist.", pageToken);
			return page;
		}

		private XmlDocument GetXmlDocument(string path)
		{
			if (xmlDocumentTempCache.ContainsKey(path))
				return xmlDocumentTempCache[path];
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(WebUtility.MapPath(path));
			}
			catch
			{
				return null;
			}
			return doc;
		}

		private object RenderPage(ExecutionState state)
		{
			Token prevToken = state.SourceToken;
			state.SourceToken = pageToken;
			string page = GetPage(state).Render(state);
			state.SourceToken = prevToken;
			if (popStack)
				ContentManager.PageStack.Pop();
			return page;
		}
		private object RenderPagePath(ExecutionState state)
		{
			string str = GetPage(state).Path;
			if (popStack)
				ContentManager.PageStack.Pop();
			return str;
		}
		private object RenderPageXML(ExecutionState state)
		{
			string path = GetPage(state).ContentFile;
			if (path == "")
				throw new InstructionExecutionException("I can't get the XML content for the page because the Page element in the definitions.xml file doesn't have a ContentFile attribute.", xpathToken);

			string text = "";
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(WebUtility.MapPath(path));
			}
			catch (Exception ex)
			{
				throw new InstructionExecutionException("I can't get the XML content for the page because the specified XML content file (" + path + ") doesn't seem to exist.", ex, xpathToken);
			}
			XmlNodeList nodes;
			try
			{
				nodes = doc.SelectNodes(xpathExpr);
			}
			catch (Exception ex)
			{
				throw new InstructionExecutionException("I can't get the XML content for the page because the XPath expression has an error.", ex, xpathToken);
			}
			foreach (XmlNode node in nodes)
			{
				SprocketScript script = null;
				if (node.NodeType == XmlNodeType.CDATA || node.NodeType == XmlNodeType.Text)
					script = new SprocketScript(node.Value, path, path + ":{" + xpathExpr + "}");
				else if (node.HasChildNodes)
					script = new SprocketScript(node.FirstChild.Value, path, path + ":{" + xpathExpr + "}");
				if (script != null)
				{
					//string str = node.FirstChild.Value;
					//foreach (PlaceHolder ph in PlaceHolder.Extract(str))
					//{
					//    str = str.Replace(ph.RawText, ph.Render(pageEntry, content, placeHolderStack, out cache));
					//    cacheable = cacheable && cache;
					//}
					//try
					//{
						text += script.ExecuteToResolveExpression(state);
					//    if(script.HasParseError)
					//        throw new InstructionExecutionException(script.Exception.Message, script.Exception, xpathToken);
					//}
					//catch (TokeniserException ex)
					//{
						
					//}
				}
			}
			return text;
		}
	}
	public class PageExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "page"; } }
		public IExpression Create() { return new PageExpression(); }
	}
}
