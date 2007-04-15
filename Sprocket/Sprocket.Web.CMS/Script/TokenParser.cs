using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script.Parser
{
	public class TokenParserException : Exception
	{
		private Token token;

		public Token Token
		{
			get { return token; }
		}

		public TokenParserException(string message, Token token)
			: base(message)
		{
			this.token = token;
		}
	}

	public static class TokenParser
	{
		private static Dictionary<string, IInstructionCreator> instructionCreators;
		private static Dictionary<string, IBinaryExpressionCreator> binaryExpressionCreators;
		private static Dictionary<string, IExpressionCreator> expressionCreators;
		private static Dictionary<string, IFilterExpressionCreator> filterExpressionCreators;

		static TokenParser()
		{
			instructionCreators = new Dictionary<string, IInstructionCreator>();
			binaryExpressionCreators = new Dictionary<string, IBinaryExpressionCreator>();
			expressionCreators = new Dictionary<string, IExpressionCreator>();
			filterExpressionCreators = new Dictionary<string, IFilterExpressionCreator>();

			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IInstructionCreator)))
			{
				IInstructionCreator ic = (IInstructionCreator)Activator.CreateInstance(t);
				instructionCreators.Add(ic.Keyword.ToLower(), ic);
			}

			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IBinaryExpressionCreator)))
			{
				IBinaryExpressionCreator bxc = (IBinaryExpressionCreator)Activator.CreateInstance(t);
				binaryExpressionCreators.Add(bxc.Keyword.ToLower(), bxc);
			}

			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IExpressionCreator)))
			{
				IExpressionCreator xc = (IExpressionCreator)Activator.CreateInstance(t);
				expressionCreators.Add(xc.Keyword.ToLower(), xc);
			}

			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(IFilterExpressionCreator)))
			{
				IFilterExpressionCreator fxc = (IFilterExpressionCreator)Activator.CreateInstance(t);
				filterExpressionCreators.Add(fxc.Keyword.ToLower(), fxc);
			}
		}

		public static IInstruction BuildInstruction(List<Token> tokens)
		{
			int n = 0;
			return BuildInstruction(tokens, ref n);
		}

		public static IInstruction BuildInstruction(List<Token> tokens, ref int nextIndex)
		{
			// if we're just starting, encase the root-level instructions with a section statement
			if (nextIndex == 0)
			{
				tokens.Insert(0, new Token(InstructionList.Keyword, TokenType.Word, 0));
				tokens.Add(new Token("end", TokenType.Word, 0));
				nextIndex++;
				InstructionList il = new InstructionList();
				il.Build(tokens, ref nextIndex);
				return il;
			}

			Token token = tokens[nextIndex];

			// special case: if a standalone literal is found, create a "show" instruction to process it
			if (token.TokenType == TokenType.StringLiteral)
			{
				if (!token.IsNonScriptText)
					throw new TokenParserException("What's this quote here for?", token);
				ShowInstruction si = new ShowInstruction();
				si.Build(tokens, ref nextIndex, true);
				return si;
			}

			//if(token.TokenType == TokenType.GroupEnd)
				

			// find the relevant creator/processor for the instruction
			if (token.TokenType == TokenType.Word || token.TokenType == TokenType.Symbolic)
			{
				if (instructionCreators.ContainsKey(token.Value))
				{
					IInstruction instruction = instructionCreators[token.Value].Create();
					nextIndex++;
					instruction.Build(tokens, ref nextIndex);
					return instruction;
				}
			}

			if(token.TokenType == TokenType.GroupStart || token.TokenType == TokenType.GroupEnd)
				throw new TokenParserException("Not sure why there is a bracket here.", token);

			throw new TokenParserException("I have no idea what \"" + token.Value + "\" means or at least what I'm supposed to do with it here.", token);
		}

		public static IExpression BuildExpression(List<Token> tokens, ref int nextIndex)
		{
			Stack<int?> stack = new Stack<int?>();
			stack.Push(null);
			return BuildExpression(tokens, ref nextIndex, stack);
		}

		public static IExpression BuildExpression(List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
			if (nextIndex >= tokens.Count)
				throw new TokenParserException("The script seems to have ended prematurely while I was doing calculations. Shouldn't there be something here?", tokens[tokens.Count - 1]);

			Token token = tokens[nextIndex];
			IExpression expr = null;
			bool endGroupedExpression = false;
			switch (token.TokenType)
			{
				case TokenType.Number:
				case TokenType.StringLiteral:
					expr = BuildSingularExpression(tokens, ref nextIndex);
					break;

				case TokenType.Word:
					if (!expressionCreators.ContainsKey(token.Value))
					{
						AssertNotEndOfList(tokens, nextIndex);
						if (tokens[nextIndex + 1].Value == ":" && tokens[nextIndex + 1].TokenType == TokenType.Symbolic)
						{
							nextIndex+=2;
							AssertNotEndOfList(tokens, nextIndex);
							Token propertyNameToken = tokens[nextIndex++];
							if (token.TokenType != TokenType.Word)
								throw new TokenParserException("The \":\" symbol indicates that you want the value of a property of the preceding variable. In this case, you seem to have forgotten to put in the property name. e.g. \"variablename:property_name\"", propertyNameToken);
							expr = new VariableExpression(token.Value, propertyNameToken.Value, token, propertyNameToken);
							if (tokens[nextIndex].TokenType == TokenType.GroupStart)
							{
								Token indexToken = tokens[nextIndex];
								((VariableExpression)expr).SetListIndex(BuildGroupedExpression(tokens, ref nextIndex), indexToken);
							}
						}
						else
						{
							expr = new VariableExpression(token.Value, token);
							nextIndex++;
						}
						break;
					}
					else
					{
						expr = expressionCreators[token.Value].Create();
						nextIndex++;
						if (expr is IObjectExpression)
						{
							if (tokens[nextIndex].TokenType == TokenType.Symbolic && tokens[nextIndex].Value == ":")
							{
								Token propertyToken = tokens[++nextIndex];
								if (propertyToken.TokenType != TokenType.Word)
									throw new TokenParserException("This point in the script should be a word indicating a property of the preceding object.", propertyToken);
								nextIndex++;
								if (!((IObjectExpression)expr).PrepareProperty(propertyToken, tokens, ref nextIndex))
									throw new TokenParserException("\"" + propertyToken.Value + "\" is not a valid property for this object.", propertyToken);
							}
						}
						if (expr is IFunctionExpression)
							((IFunctionExpression)expr).SetArguments(BuildFunctionArgumentList(tokens, ref nextIndex), token);
						expr.PrepareExpression(token, tokens, ref nextIndex, precedenceStack);
					}
					break;

				case TokenType.GroupStart:
					expr = BuildGroupedExpression(tokens, ref nextIndex);
				    break;

				case TokenType.GroupEnd:
					endGroupedExpression = true;
					break;

				default:
					throw new TokenParserException("This part of the script should equate to a value but instead I got \"" + token.Value + "\", which doesn't really mean anything in this context.", token);
			}

			if (!endGroupedExpression)
			{
				int? precedence = precedenceStack.Peek();
				while (NextHasGreaterPrecedence(precedence, tokens, nextIndex))
					expr = BuildBinaryExpression(tokens, ref nextIndex, expr, precedenceStack);
				//if (index < tokens.Count - 1)
				//    if (tokens[index].TokenType == TokenType.GroupEnd)
				//        index++;
			}
			return expr;
		}

		public static IExpression BuildSingularExpression(List<Token> tokens, ref int nextIndex)
		{
			Stack<int?> stack = new Stack<int?>();
			return BuildSingularExpression(tokens, ref nextIndex, stack);
		}

		public static IExpression BuildSingularExpression(List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
			Token token = tokens[nextIndex];
			IExpression expr;
			switch (token.TokenType)
			{
				case TokenType.Number:
					expr = new NumericExpression();
					break;

				case TokenType.StringLiteral:
					expr = new StringExpression();
					break;

				default:
					expr = BuildExpression(tokens, ref nextIndex, precedenceStack);
					break;
			}

			expr.PrepareExpression(token, tokens, ref nextIndex, precedenceStack);
			return expr;
		}

		public static IExpression BuildGroupedExpression(List<Token> tokens, ref int nextIndex)
		{
			nextIndex++;
			IExpression expr = BuildExpression(tokens, ref nextIndex);
			if (tokens[nextIndex].TokenType != TokenType.GroupEnd)
			{
				string tokenval = tokens[nextIndex].Value.Trim();
				if (tokenval == "")
					tokenval = "#";
				throw new TokenParserException("I think a closing bracket should be here. Did you forget to put it in?", new Token(tokenval, TokenType.Word, tokens[nextIndex].Position));
			}
			nextIndex++;
			return expr;
		}

		public static List<FunctionArgument> BuildFunctionArgumentList(List<Token> tokens, ref int nextIndex)
		{
			List<FunctionArgument> args = new List<FunctionArgument>();
			if (tokens[nextIndex].TokenType != TokenType.GroupStart)
				return args;
			nextIndex++;
			while (tokens[nextIndex].TokenType != TokenType.GroupEnd)
			{
				Token token = tokens[nextIndex];
				IExpression expr = BuildExpression(tokens, ref nextIndex);
				if (nextIndex >= tokens.Count)
					throw new TokenParserException("Oops, looks like someone didn't finish writing the script. It ended while I was putting together a list of arguments for a function call.", tokens[tokens.Count-1]);
				args.Add(new FunctionArgument(expr, token));
				token = tokens[nextIndex];
				if (token.TokenType == TokenType.GroupEnd)
					continue;
				if (token.TokenType == TokenType.Symbolic && token.Value == ",")
					nextIndex++;
				else
					throw new TokenParserException("The list of function arguments needs to either end with a closing bracket, or have a comma to indicate that another argument is next.", token);
			}
			nextIndex++;
			return args;
		}

		public static IExpression BuildBinaryExpression(List<Token> tokens, ref int nextIndex, IExpression leftExpr, Stack<int?> precedenceStack)
		{
			IBinaryExpressionCreator bxc = binaryExpressionCreators[tokens[nextIndex++].Value];
			IBinaryExpression bx = bxc.Create();
			bx.Left = leftExpr;
			precedenceStack.Push(bx.Precedence);
			bx.PrepareExpression(tokens[nextIndex], tokens, ref nextIndex, precedenceStack);
			precedenceStack.Pop();
			return bx;
		}

		public static bool NextHasGreaterPrecedence(int? thanPrecedence, List<Token> tokens, int nextIndex)
		{
			if (nextIndex >= tokens.Count)
				return false;
			Token token = tokens[nextIndex];
			if (token.TokenType != TokenType.Symbolic && token.TokenType != TokenType.Word)
				return false;
			if (!binaryExpressionCreators.ContainsKey(token.Value))
				return false;
			if (thanPrecedence == null) // previous check must come before this one or we'll get a default true even if the operator (e.g. "@") isn't defined as a standard binary expression
				return true;
			return binaryExpressionCreators[token.Value].Precedence < thanPrecedence.Value;
		}

		internal static IFilterExpression BuildFilterExpression(List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
			Token token = tokens[nextIndex++];
			if (token.TokenType == TokenType.Word)
			{
				if (filterExpressionCreators.ContainsKey(token.Value))
				{
					IFilterExpression fx = filterExpressionCreators[token.Value].Create();
					fx.PrepareExpression(tokens[nextIndex], tokens, ref nextIndex, precedenceStack);
					return fx;
				}
			}
			throw new InstructionExecutionException("I was expecting something here that can take the value on the left and change the way it looks. \"" + token.Value + "\" doesn't have that capability.", token);
		}

		public static void AssertNotEndOfList(List<Token> list, int nextIndex)
		{
			if (list.Count <= nextIndex)
				throw new TokenParserException("I seem to have come to the end of the script prematurely. Should something be here?", list[list.Count - 1]);
		}

		public static object VerifyUnderlyingType(object o)
		{
			if (o == null)
				return new BooleanExpression.SoftBoolean(false);
			if (o is int || o is short || o is long || o is float || o is double || o is ushort || o is ulong || o is uint)
				return Convert.ToDecimal(o);
			return o;
		}
	}
}
