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
		private IExpression xpathExpr = null;
		private IExpression pageCodeExpr = null;
		private Token pageToken;
		private Token xpathToken;
		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			pageToken = tokens[index - 1];
			Token token = tokens[index];
			if (token.TokenType == TokenType.Symbolic)
				if (token.Value == ":")
				{
					index++;
					token = tokens[index++];
					if (token.TokenType == TokenType.Word)
					{
						switch (token.Value)
						{
							case "path":
								RenderValue = RenderPagePath;
								break;

							//default:
							//    throw new TokenParserException("I can't get the page info requested here because \"" + token.Value + "\" isn't an attribute of the specified page.", token);
						}
					}
					if (RenderValue == null)
					{
						xpathExpr = TokenParser.BuildExpression(tokens, ref index, precedenceStack);
						xpathToken = token;
						RenderValue = RenderPageXML;
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
			object xpath = xpathExpr.Evaluate(state).ToString();
			string path = GetPage(state).ContentFile;
			if (path == "")
				throw new InstructionExecutionException("I can't get the XML content for the page because the Page element in the definitions.xml file doesn't have a ContentFile attribute.", pageToken);

			string text = "";
			XmlNodeList nodes;
			try
			{
				nodes = content.SelectNodes(xpath);
			}
			catch(Exception ex)
			{
				throw new InstructionExecutionException("I can't get the XML content for the page because the XPath expression has an error.", ex, pageToken);
			}
			foreach (XmlNode node in nodes)
			{
				SprocketScript script = null;
				if (node.NodeType == XmlNodeType.CDATA || node.NodeType == XmlNodeType.Text)
					script = new SprocketScript(node.Value, path, path + ":{" + xpath + "}");
				else if(node.HasChildNodes)
					script = new SprocketScript(node.FirstChild.Value, path, path + ":{" + xpath + "}");
				if(script != null)
					
				//string str = node.FirstChild.Value;
				//foreach (PlaceHolder ph in PlaceHolder.Extract(str))
				//{
				//    str = str.Replace(ph.RawText, ph.Render(pageEntry, content, placeHolderStack, out cache));
				//    cacheable = cacheable && cache;
				//}
				text += str;
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
