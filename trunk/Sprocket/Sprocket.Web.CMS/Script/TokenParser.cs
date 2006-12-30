using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script.Parser
{
	internal class TokenParserException : Exception
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

	public class ExecutionState
	{
		private Stack<string> scriptNameStack = new Stack<string>();
		public Stack<string> ScriptNameStack
		{
			get { return scriptNameStack; }
		}

		private Stack<SprocketScript> executingScript = new Stack<SprocketScript>();
		public Stack<SprocketScript> ExecutingScript
		{
			get { return executingScript; }
		}

		private StreamWriter output;
		public StreamWriter Output
		{
			get { return output; }
		}

		public ExecutionState(Stream stream)
		{
			output = new StreamWriter(stream);
			output.AutoFlush = true;
		}

		private Dictionary<string, SprocketScript> sectionOverrides = new Dictionary<string, SprocketScript>();
		public Dictionary<string, SprocketScript> SectionOverrides
		{
			get { return sectionOverrides; }
			set { sectionOverrides = value; }
		}
	}

	internal static class TokenParser
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

		public static IInstruction BuildInstruction(List<Token> tokens, ref int index)
		{
			// if we're just starting, encase the root-level instructions with a section statement
			if (index == 0)
			{
				tokens.Insert(0, new Token(InstructionList.Keyword, TokenType.Word, 0));
				tokens.Add(new Token("end", TokenType.Word, 0));
				index++;
				InstructionList il = new InstructionList();
				il.Build(tokens, ref index);
				return il;
			}

			Token token = tokens[index];

			// special case: if a standalone literal is found, create a "show" instruction to process it
			if (token.TokenType == TokenType.StringLiteral)
			{
				ShowInstruction si = new ShowInstruction();
				si.Build(tokens, ref index, true);
				return si;
			}

			// find the relevant creator/processor for the instruction
			if (token.TokenType == TokenType.Word || token.TokenType == TokenType.Symbolic)
			{
				if (instructionCreators.ContainsKey(token.Value))
				{
					IInstruction instruction = instructionCreators[token.Value].Create();
					index++;
					instruction.Build(tokens, ref index);
					return instruction;
				}
			}

			throw new TokenParserException("I have no idea what \"" + token.Value + "\" means or at least what I'm supposed to do with it here.", token);
		}

		public static IExpression BuildExpression(List<Token> tokens, ref int index)
		{
			Stack<int?> stack = new Stack<int?>();
			stack.Push(null);
			return BuildExpression(tokens, ref index, stack);
		}

		public static IExpression BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			if (index >= tokens.Count)
				throw new TokenParserException("The script seems to have ended prematurely while I was doing calculations. Shouldn't there be something here?", tokens[tokens.Count - 1]);

			Token token = tokens[index];
			IExpression expr = null;
			switch (token.TokenType)
			{
				case TokenType.Number:
				case TokenType.StringLiteral:
					expr = BuildSingularExpression(tokens, ref index);
					break;

				case TokenType.Word:
					if (token.Value == "true" || token.Value == "false")
					{
						expr = new BooleanExpression();
						expr.BuildExpression(tokens, ref index, precedenceStack);
					}
					else
						expr = BuildWordExpression(tokens, ref index, precedenceStack);
					break;

				//case TokenType.GroupStart:
				//    expr = BuildGroupedExpression(tokens, ref index);
				//    break;

				default:
					throw new TokenParserException("This part of the script should equate to a value but instead I got \"" + token.Value + "\", which doesn't really mean anything in this context.", token);
			}

			int? precedence = precedenceStack.Peek();
			while (NextHasGreaterPrecedence(precedence, tokens, index))
				expr = BuildBinaryExpression(tokens, ref index, expr, precedenceStack);

			return expr;
		}

		public static IExpression BuildSingularExpression(List<Token> tokens, ref int index)
		{
			Stack<int?> stack = new Stack<int?>();
			return BuildSingularExpression(tokens, ref index, stack);
		}

		public static IExpression BuildSingularExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			Token token = tokens[index];
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
					expr = BuildExpression(tokens, ref index, precedenceStack);
					break;
			}

			expr.BuildExpression(tokens, ref index, precedenceStack);
			return expr;
		}

		public static IExpression BuildGroupedExpression(List<Token> tokens, ref int index)
		{
			return null;
		}

		public static IExpression BuildWordExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			Token token = tokens[index++];
			if (!expressionCreators.ContainsKey(token.Value))
				throw new TokenParserException("I can't complete the calculations because \"" + token.Value + "\" doesn't equate to anything I can use in this situation.", token);
			IExpression expr = expressionCreators[token.Value].Create();
			expr.BuildExpression(tokens, ref index, precedenceStack);
			return expr;
		}

		public static IExpression BuildBinaryExpression(List<Token> tokens, ref int index, IExpression leftExpr, Stack<int?> precedenceStack)
		{
			IBinaryExpressionCreator bxc = binaryExpressionCreators[tokens[index++].Value];
			IBinaryExpression bx = bxc.Create();
			bx.Left = leftExpr;
			precedenceStack.Push(bx.Precedence);
			bx.BuildExpression(tokens, ref index, precedenceStack);
			precedenceStack.Pop();
			return bx;
		}

		public static bool NextHasGreaterPrecedence(int? thanPrecedence, List<Token> tokens, int index)
		{
			if (index >= tokens.Count)
				return false;
			Token token = tokens[index];
			if (token.TokenType != TokenType.Symbolic && token.TokenType != TokenType.Word)
				return false;
			if (!binaryExpressionCreators.ContainsKey(token.Value))
				return false;
			if (thanPrecedence == null) // previous check must come before this one or we'll get a default true even if the operator (e.g. "@") isn't defined as a standard binary expression
				return true;
			return binaryExpressionCreators[token.Value].Precedence < thanPrecedence.Value;
		}

		internal static IFilterExpression BuildFilterExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			Token token = tokens[index++];
			if (token.TokenType == TokenType.Word)
			{
				if (filterExpressionCreators.ContainsKey(token.Value))
				{
					IFilterExpression fx = filterExpressionCreators[token.Value].Create();
					fx.BuildExpression(tokens, ref index, precedenceStack);
					return fx;
				}
			}
			throw new InstructionExecutionException("I was expecting something here that can take the value on the left and change the way it looks. \"" + token.Value + "\" doesn't have that capability.", token);
		}
	}
}
