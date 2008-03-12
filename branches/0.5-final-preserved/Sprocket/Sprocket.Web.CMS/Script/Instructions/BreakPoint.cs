using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	internal class BreakPointInstruction : IInstruction
	{
		private Token token;

		public BreakPointInstruction() { }

		public void Build(TokenList tokens)
		{
			token = tokens.Current;
			// advance past the instruction token/symbol
			tokens.Advance();
		}

		public void Execute(ExecutionState state)
		{
			if (true)
			{
			}
		}
	}

	internal class BreakPointInstructionCreator : IInstructionCreator
	{
		public string Keyword { get { return "breakpoint"; } }
		public IInstruction Create() { return new BreakPointInstruction(); }
	}
}
