using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.SprocketScript.Parser
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
		void Build(List<Token> tokens, ref int index);
		bool Execute(ExecutionState state);
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
		public void Build(List<Token> tokens, ref int index)
		{
			while (index < tokens.Count)
			{
				Token token = tokens[index];
				if (Token.IsEnd(token))
				{
					index++;
					return;
				}
				list.Add(TokenParser.BuildInstruction(tokens, ref index));
			}
		}

		public bool Execute(ExecutionState state)
		{
			foreach (IInstruction instruction in list)
				if (!instruction.Execute(state))
					return false;
			return true;
		}
	}

	internal class InstructionListCreator : IInstructionCreator
	{
		public string Keyword
		{
			get { return InstructionList.Keyword; }
		}

		public IInstruction Create()
		{
			return new InstructionList();
		}
	}

	#endregion

	#region ShowInstruction

	internal class ShowInstruction : IInstruction
	{
		private IExpression expression = null;

		public const string Keyword = "show";

		public void Build(List<Token> tokens, ref int index)
		{
			Build(tokens, ref index, false);
		}

		public void Build(List<Token> tokens, ref int index, bool useSingularExpression)
		{
			if (useSingularExpression)
				expression = TokenParser.BuildSingularExpression(tokens, ref index);
			else
				expression = TokenParser.BuildExpression(tokens, ref index);
		}

		public bool Execute(ExecutionState state)
		{
			string text = expression.Evaluate(state).ToString();
			state.Output.Write(text);
			return true;
		}
	}

	internal class ShowInstructionCreator : IInstructionCreator
	{
		public string Keyword
		{
			get { return ShowInstruction.Keyword; }
		}

		public IInstruction Create()
		{
			return new ShowInstruction();
		}
	}

	#endregion
}
