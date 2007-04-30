using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sprocket.Web.CMS.Script
{
	public class IfInstruction : IInstruction
	{
		private InstructionList whenTrue = null, whenFalse = null;
		private IExpression expr = null;
		private Token token;
		public void Build(TokenList tokens)
		{
			token = tokens.Current;
			// advance past the instruction token/symbol
			tokens.Advance();

			// build condition expression
			expr = TokenParser.BuildExpression(tokens);

			// build the instruction list that will execute if the expression evaluates to true
			whenTrue = new InstructionList();
			whenTrue.AcceptELSEInPlaceOfEND = true;
			whenTrue.Build(tokens);

			// if an else statement was used, build the instruction list that will execute if the expression evaluates to false
			if (whenTrue.Terminator == InstructionList.TerminatorType.Else)
			{
				whenFalse = new InstructionList();
				whenFalse.Build(tokens);
			}
		}

		public void Execute(ExecutionState state)
		{
			object val = expr.Evaluate(state, token);
			if (BooleanExpression.True.Equals(val))
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
}
