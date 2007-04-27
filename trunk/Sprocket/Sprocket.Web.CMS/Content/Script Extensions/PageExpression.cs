using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;
using System.Xml;

namespace Sprocket.Web.CMS.Content.Expressions
{
	public class PageExpression : IObjectExpression
	{
		public enum Property
		{
			None,
			Path,
			Code
		}

		private delegate object RenderHandler(ExecutionState state);
		private RenderHandler RenderValue;
		private Token functionToken;

		//private string xpathExpr = null;
		//private Token xpathToken;
		//private Token pageCodeToken;

		private List<FunctionArgument> arguments = null;
		private Property property = Property.None;

		public PageExpression()
		{
			RenderValue = RenderPage;
		}

		private static Dictionary<string, XmlDocument> xmlDocumentTempCache = new Dictionary<string, XmlDocument>();

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
			//pageToken = tokens[nextIndex - 1];
			//Token token = tokens[nextIndex];
			//if (token.TokenType == TokenType.Symbolic)
			//{
			//    // if the token is a colon, then we're extracting specific information relating to the page
			//    if (token.Value == ":")
			//    {
			//        nextIndex++;
			//        token = tokens[nextIndex++];
			//        // if it's a word, see what the word is
			//        if (token.TokenType == TokenType.Word)
			//        {
			//            switch (token.Value)
			//            {
			//                case "path": // get the sprocket path for the page
			//                    RenderValue = RenderPagePath;
			//                    break;

			//                //default:
			//                //    throw new TokenParserException("I can't get the page info requested here because \"" + token.Value + "\" isn't an attribute of the specified page.", token);
			//            }
			//        } // otherwise if it's a string, it's an xpath expression
			//        else if (token.TokenType == TokenType.StringLiteral && !token.IsNonScriptText)
			//        {
			//            xpathExpr = token.Value; //TokenParser.BuildExpression(tokens, ref index, precedenceStack);
			//            xpathToken = token;
			//            RenderValue = RenderPageXML;
			//        }
			//    }
			//}

			//token = tokens[nextIndex];
			//if ((token.TokenType == TokenType.StringLiteral && !token.IsNonScriptText) || token.TokenType == TokenType.GroupStart)
			//{
			//    pageCodeExpr = TokenParser.BuildExpression(tokens, ref nextIndex, precedenceStack, true);
			//    pageCodeToken = token;
			//}

			//if (RenderValue == null)
			//    RenderValue = RenderPage;
		}

		public void SetFunctionArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			this.functionToken = functionCallToken;
			this.arguments = arguments;
		}

		public bool PresetPropertyName(Token propertyToken, List<Token> tokens, ref int nextIndex)
		{
			switch (propertyToken.Value)
			{
				case "path":
					property = Property.Path;
					RenderValue = RenderPagePath;
					break;

				case "code":
					property = Property.Code;
					RenderValue = RenderPageCode;
					break;

				default:
					return false;
			}
			return true;
		}

		public object Evaluate(ExecutionState state)
		{
			return RenderValue(state);
		}

		bool popStack;
		private PageEntry GetPage(FunctionArgument arg, ExecutionState state)
		{
			if (arg == null)
			{
				popStack = false;
				if (ContentManager.PageStack.Count == 0)
					throw new InstructionExecutionException("The current page was requested but somebody forgot to note down what the current page is!", functionToken);
				else
					return ContentManager.PageStack.Peek();
			}
			popStack = true;
			PageEntry page = ContentManager.Pages.FromPageCode(arg.Expression.Evaluate(state).ToString());
			ContentManager.PageStack.Push(page);
			if (page == null)
				throw new InstructionExecutionException("I can't get information about the page because the one specified doesn't seem to exist.", arg.Token);
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
			if (arguments.Count > 1)
				throw new InstructionExecutionException("Too many arguments specified. Specify none, or just one indicating the page code you are referring to.", functionToken);
			FunctionArgument codeArg = arguments.Count == 0 ? null : arguments[0];
			Token prevToken = state.SourceToken;
			state.SourceToken = functionToken;
			string page = GetPage(codeArg, state).Render(state);
			state.SourceToken = prevToken;
			if (popStack)
				ContentManager.PageStack.Pop();
			return page;
		}

		private object RenderPagePath(ExecutionState state)
		{
			if (arguments.Count > 1)
				throw new InstructionExecutionException("Too many arguments specified. Specify none, or just one indicating the page code you are referring to.", functionToken);
			FunctionArgument codeArg = arguments.Count == 0 ? null : arguments[0];
			string str = GetPage(codeArg, state).Path;
			if (popStack)
				ContentManager.PageStack.Pop();
			return str;
		}

		private object RenderPageCode(ExecutionState state)
		{
			if (arguments.Count > 0)
				throw new InstructionExecutionException("Don't specify any arguments when requesting the page code. That would be like saying \"give me the color of blue\". :P", functionToken);
			FunctionArgument codeArg = arguments.Count == 0 ? null : arguments[0];
			string str = GetPage(codeArg, state).PageCode;
			if (popStack)
				ContentManager.PageStack.Pop();
			return str;
		}
		//private object RenderPageXML(ExecutionState state)
		//{
		//    string path = GetPage(state).ContentFile;
		//    if (path == "")
		//        throw new InstructionExecutionException("I can't get the XML content for the page because the Page element in the definitions.xml file doesn't have a ContentFile attribute.", xpathToken);

		//    string text = "";
		//    XmlDocument doc = new XmlDocument();
		//    try
		//    {
		//        doc.Load(WebUtility.MapPath(path));
		//    }
		//    catch (Exception ex)
		//    {
		//        throw new InstructionExecutionException("I can't get the XML content for the page because the specified XML content file (" + path + ") doesn't seem to exist.", ex, xpathToken);
		//    }
		//    XmlNodeList nodes;
		//    try
		//    {
		//        nodes = doc.SelectNodes(xpathExpr);
		//    }
		//    catch (Exception ex)
		//    {
		//        throw new InstructionExecutionException("I can't get the XML content for the page because the XPath expression has an error.", ex, xpathToken);
		//    }
		//    foreach (XmlNode node in nodes)
		//    {
		//        SprocketScript script = null;
		//        if (node.NodeType == XmlNodeType.CDATA || node.NodeType == XmlNodeType.Text)
		//            script = new SprocketScript(node.Value, path, path + ":{" + xpathExpr + "}");
		//        else if (node.HasChildNodes)
		//            script = new SprocketScript(node.FirstChild.Value, path, path + ":{" + xpathExpr + "}");
		//        if (script != null)
		//        {
		//            //string str = node.FirstChild.Value;
		//            //foreach (PlaceHolder ph in PlaceHolder.Extract(str))
		//            //{
		//            //    str = str.Replace(ph.RawText, ph.Render(pageEntry, content, placeHolderStack, out cache));
		//            //    cacheable = cacheable && cache;
		//            //}
		//            //try
		//            //{
		//            text += script.ExecuteToResolveExpression(state);
		//            //    if(script.HasParseError)
		//            //        throw new InstructionExecutionException(script.Exception.Message, script.Exception, xpathToken);
		//            //}
		//            //catch (TokeniserException ex)
		//            //{

		//            //}
		//        }
		//    }
		//    return text;
		//}
	}
	public class PageExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "page"; } }
		public IExpression Create() { return new PageExpression(); }
	}
}
