using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sprocket.Web.CMS.SprocketScript.Parser
{
	internal static class Tokeniser
	{
		public static List<Token> Extract(string source)
		{
			List<Token> tokens = new List<Token>();

			if (source == null)
				return tokens;

			string sourceExpr = @"\{\?\s*(?<expr>(([^\{\}]*)|((?<=\\)[\{\}]))*)\s*\}";
			MatchCollection matches = Regex.Matches(source, sourceExpr, RegexOptions.Multiline | RegexOptions.ExplicitCapture);
			if (matches.Count == 0)
				tokens.Add(new Token(source, TokenType.StringLiteral, 0));
			else
			{
				int previousMatchEndPosition = 0;
				for (int i = 0; i < matches.Count; i++)
				{
					Match match = matches[i];
					int htmlLength = match.Index - previousMatchEndPosition;
					if (htmlLength > 0)
						tokens.Add(new Token(source.Substring(previousMatchEndPosition, htmlLength), TokenType.StringLiteral, previousMatchEndPosition));
					try
					{
						previousMatchEndPosition = match.Index + match.Length;
						string str = match.Groups["expr"].Value;
						int index = 0;

						while (index < str.Length)
						{
							Token token = ReadToken(ref str, ref index);
							if (token != null)
							{
								token.Position += match.Groups["expr"].Index;
								tokens.Add(token);
							}
						}
					}
					catch (TokeniserException ex)
					{
						int start = Math.Max(ex.Position - 100, 0);
						string err = Environment.NewLine
							+ "ERROR PARSING SPROCKETSCRIPT\r\n"
							+ "Position " + ex.Position + ": " + ex.Message + Environment.NewLine
							+ "Source: " + ex.ScriptSource.Substring(start, ex.Position - start) + "[ERROR CODE...]\r\n\r\n";
						tokens.Clear();
						tokens.Add(new Token(err, TokenType.StringLiteral, 0));
						return tokens;
					}
				}
				if (previousMatchEndPosition < source.Length)
					tokens.Add(new Token(source.Substring(previousMatchEndPosition), TokenType.StringLiteral, previousMatchEndPosition));
			}

			return tokens;
		}

		private static bool IsSymbolic(char ch)
		{
			switch (ch)
			{
				case '+':
				case '-':
				case '*':
				case '/':
				case '=':
				case '>':
				case '<':
				case '!':
				case '@':
				case '#':
				case '$':
				case '%':
				case '^':
				case '&':
				case ':':
				case '?':
					return true;

				default:
					return false;
			}
		}

		private delegate Token TokenReaderMethod(ref string source, ref int index);

		private static Token ReadToken(ref string source, ref int index)
		{
			TokenReaderMethod method;
			char ch = source[index];
			if (char.IsDigit(ch) || ch == '.')
				method = ReadNumber;
			else if (IsSymbolic(ch))
				method = ReadSymbolic;
			else if (char.IsLetter(ch) || ch == '_')
				method = ReadWord;
			else if (ch == '(')
				method = ReadGroupStart;
			else if (ch == ')')
				method = ReadGroupEnd;
			else if (char.IsWhiteSpace(ch))
				method = ReadWhiteSpace;
			else if (ch == '"')
				method = ReadStringLiteral;
			else
				throw new TokeniserException("Unexpected character found", index, source);
			return method(ref source, ref index);
		}

		private static Token ReadNumber(ref string source, ref int index)
		{
			int start = index;
			bool decimalPointAdded = false;
			bool addLeadingZero = false, addTrailingZero = true;
			while (index < source.Length)
			{
				if (source[index] == '.')
				{
					if (decimalPointAdded)
						throw new TokeniserException("Duplicate decimal point found in number", index, source);
					if (start == index)
						addLeadingZero = true;
					decimalPointAdded = true;
					index++;
					continue;
				}
				else if (!char.IsDigit(source[index]))
					break;
				index++;
				addTrailingZero = false;
			}
			return new Token((addLeadingZero ? "0" : "")
				+ source.Substring(start, index - start)
				+ (addTrailingZero ? "0" : ""), TokenType.Number, start);
		}
		private static Token ReadSymbolic(ref string source, ref int index)
		{
			int start = index;
			while (IsSymbolic(source[index]))
			{
				index++;
				if (index == source.Length)
					break;
			}
			return new Token(source.Substring(start, index - start), TokenType.Symbolic, start);
		}
		private static Token ReadWord(ref string source, ref int index)
		{
			int start = index;
			index++;
			while (char.IsLetterOrDigit(source[index]) || source[index] == '_' || source[index] == '\'')
			{
				index++;
				if (index == source.Length)
					break;
			}
			return new Token(source.Substring(start, index - start), TokenType.Word, start);
		}
		private static Token ReadGroupStart(ref string source, ref int index)
		{
			return new Token("(", TokenType.GroupStart, index++);
		}
		private static Token ReadGroupEnd(ref string source, ref int index)
		{
			return new Token(")", TokenType.GroupEnd, index++);
		}
		private static Token ReadWhiteSpace(ref string source, ref int index)
		{
			while (char.IsWhiteSpace(source[index]))
			{
				index++;
				if (index == source.Length)
					break;
			}
			return null;
		}
		private static Token ReadStringLiteral(ref string source, ref int index)
		{
			bool isEscaped = false;
			int start = index;
			index++;
			StringBuilder sb = new StringBuilder();
			while (index < source.Length)
			{
				switch (source[index])
				{
					case '"':
						if (!isEscaped)
						{
							index++;
							return new Token(sb.ToString(), TokenType.StringLiteral, start);
						}
						break;

					case '\\':
						if (!isEscaped)
						{
							isEscaped = true;
							index++;
							continue;
						}
						break;
				}
				sb.Append(source[index++]);
				isEscaped = false;
			}
			throw new TokeniserException("String literal missing a closing quote (\") character", index, source);
		}
	}

	public class Token
	{
		TokenType tokenType;
		string value;
		int position;

		public TokenType TokenType
		{
			get { return tokenType; }
		}

		public string Value
		{
			get { return value; }
		}

		public int Position
		{
			get { return position; }
			internal set { position = value; }
		}

		public Token(string value, TokenType tokenType, int position)
		{
			if (tokenType == TokenType.Word)
				this.value = value.ToLower();
			else
				this.value = value;
			this.tokenType = tokenType;
			this.position = position;
		}

		public static bool IsEnd(Token token)
		{
			return token.Value == "end";
		}

		public static bool IsElse(Token token)
		{
			return token.value == "else";
		}

		public override string ToString()
		{
			return "{" + tokenType + "; " + value + "}";
		}
	}

	public enum TokenType
	{
		None,
		GroupStart,
		GroupEnd,
		StringLiteral,
		Symbolic,
		Word,
		Number
	}

	internal class TokeniserException : Exception
	{
		private int position;
		private string scriptSource;

		public string ScriptSource
		{
			get { return scriptSource; }
		}

		public int Position
		{
			get { return position; }
		}

		public TokeniserException(string message, int position, string source)
			: base(message)
		{
			this.position = position;
			this.scriptSource = source;
		}
	}
}
