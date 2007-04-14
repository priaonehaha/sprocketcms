using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script.Parser
{
	public class InstructionExecutionException : Exception
	{
		private Token token;
		public Token Token
		{
			get { return token; }
		}

		public InstructionExecutionException(string message, Token token)
			: base(message)
		{
			this.token = token;
		}

		public InstructionExecutionException(string message, Exception ex, Token token)
			: base(message, ex)
		{
			this.token = token;
		}
	}

	public interface IInstruction
	{
		/// <summary>
		/// Builds an instruction tree
		/// </summary>
		/// <param name="tokens">The full token list created by the Tokeniser</param>
		/// <param name="index">The index of the first token after the token that represents this instruction.
		/// For example, if this is a "show" instruction which exists at token list index 10, then index should be 11.
		/// After processing, index should be left at the first position not processed by either directly or indirectly
		/// (recursively) by this instruction</param>
		void Build(List<Token> tokens, ref int nextIndex);
		void Execute(ExecutionState state);
	}

	public interface IInstructionCreator
	{
		string Keyword { get; }
		IInstruction Create();
	}

	#region InstructionList

	internal class InstructionList : IInstruction
	{
		public const string Keyword = "section";
		List<IInstruction> list = new List<IInstruction>();
		private bool acceptELSEInPlaceOfEND = false;
		private string name = null;
		private Token instructionListToken = null;

		public string Name
		{
			get { return name; }
		}

		public bool AcceptELSEInPlaceOfEND
		{
			get { return acceptELSEInPlaceOfEND; }
			set { acceptELSEInPlaceOfEND = value; }
		}

		public void Build(List<Token> tokens, ref int nextIndex)
		{
			instructionListToken = tokens[nextIndex - 1];
			if (nextIndex < tokens.Count - 1)
				if (tokens[nextIndex].TokenType == TokenType.StringLiteral && !tokens[nextIndex].IsNonScriptText)
					name = tokens[nextIndex++].Value;
			while (nextIndex < tokens.Count)
			{
				Token token = tokens[nextIndex];
				if (Token.IsEnd(token) || (acceptELSEInPlaceOfEND && Token.IsElse(token)))
				{
					nextIndex++;
					return;
				}
				list.Add(TokenParser.BuildInstruction(tokens, ref nextIndex));
			}
		}

		public void Execute(ExecutionState state)
		{
			if(name != null)
				if (state.SectionOverrides.ContainsKey(name))
				{
					Token prevToken = state.SourceToken;
					state.SourceToken = instructionListToken;
					state.SectionOverrides[name].ExecuteInParentContext(state);
					state.SourceToken = prevToken;
					return;
				}
			foreach (IInstruction instruction in list)
				instruction.Execute(state);
		}
	}

	internal class InstructionListCreator : IInstructionCreator
	{
		public string Keyword { get { return InstructionList.Keyword; } }
		public IInstruction Create() { return new InstructionList(); }
	}

	internal class InstructionListCreator2 : IInstructionCreator
	{
		public string Keyword { get { return "@"; } }
		public IInstruction Create() { return new InstructionList(); }
	}

	#endregion

	#region ShowInstruction

	internal class ShowInstruction : IInstruction
	{
		private IExpression expression = null;

		public void Build(List<Token> tokens, ref int nextIndex)
		{
			Build(tokens, ref nextIndex, false);
		}

		public void Build(List<Token> tokens, ref int nextIndex, bool useSingularExpression)
		{
			if (useSingularExpression)
				expression = TokenParser.BuildSingularExpression(tokens, ref nextIndex);
			else
				expression = TokenParser.BuildExpression(tokens, ref nextIndex);
		}

		public void Execute(ExecutionState state)
		{
			object text = expression.Evaluate(state);
			state.Output.Write(text.ToString());
		}
	}

	internal class ShowInstructionCreator : IInstructionCreator
	{
		public string Keyword { get { return "show"; } }
		public IInstruction Create() { return new ShowInstruction(); }
	}

	internal class ShowInstructionCreator2 : IInstructionCreator
	{
		public string Keyword { get { return "?"; } }
		public IInstruction Create() { return new ShowInstruction(); }
	}

	#endregion

	#region IfInstruction

	public class IfInstruction : IInstruction
	{
		private InstructionList whenTrue = null, whenFalse = null;
		private IExpression expr = null;
		public void Build(List<Token> tokens, ref int nextIndex)
		{
			expr = TokenParser.BuildExpression(tokens, ref nextIndex);
			whenTrue = new InstructionList();
			whenTrue.AcceptELSEInPlaceOfEND = true;
			whenTrue.Build(tokens, ref nextIndex);
			if (Token.IsElse(tokens[nextIndex - 1]))
			{
				whenFalse = new InstructionList();
				whenFalse.Build(tokens, ref nextIndex);
			}
		}

		public void Execute(ExecutionState state)
		{
			object val = expr.Evaluate(state);
			if (val.Equals(true))
				whenTrue.Execute(state);
			else if (whenFalse != null)
				whenFalse.Execute(state);
		}
	}

	public class IfInstructionCreator : IInstructionCreator
	{
		public string Keyword { get { return "if"; } }
		public IInstruction Create() { return new IfInstruction(); }
	}

	#endregion
}
