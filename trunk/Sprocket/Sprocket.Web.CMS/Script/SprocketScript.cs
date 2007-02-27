using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Script
{
	public sealed class SprocketScript
	{
		private IInstruction instruction;
		private string source;
		private ExecutionState.ScriptRecursionIdentifier identifier;
		bool hasError = false;
		TokeniserException exception = null;

		internal TokeniserException Exception
		{
			get { return exception; }
		}
		
		public bool HasParseError
		{
			get { return hasError; }
			set { hasError = value; }
		}

		public string Source
		{
			get { return source; }
		}

		public SprocketScript(string source, string descriptiveName, string scriptIdentificationString)
		{
			this.source = source;
			this.identifier = new ExecutionState.ScriptRecursionIdentifier(descriptiveName, scriptIdentificationString);

			List<Token> tokens;
			try
			{
				tokens = Tokeniser.Extract(source);
			}
			catch (TokeniserException ex)
			{
				Token falseToken = new Token(source.Substring(ex.Position, 1), TokenType.StringLiteral, ex.Position);
				tokens = new List<Token>();
				Token token = new Token(GetErrorHTML(ex.Message, falseToken, null), TokenType.StringLiteral, 0);
				token.IsNonScriptText = true;
				tokens.Add(token);
				instruction = new ShowInstruction();
				int n = 0;
				instruction.Build(tokens, ref n);
				hasError = true;
				exception = ex;
			}

			try
			{
				instruction = TokenParser.BuildInstruction(tokens);
			}
			catch (TokenParserException ex)
			{
				tokens.Clear();
				Token token = new Token(GetErrorHTML(ex.Message, ex.Token, null), TokenType.StringLiteral, 0);
				token.IsNonScriptText = true;
				tokens.Add(token);
				instruction = new ShowInstruction();
				int n = 0;
				instruction.Build(tokens, ref n);
				hasError = true;
			}
		}

		string GetErrorHTML(string message, Token token, ExecutionState state)
		{
			int start = Math.Max(token.Position - 150, 0);
			string source = state == null ? Source : state.ExecutingScript.Peek().Source;
			string code = source.Substring(start, token.Position - start);
			string prefix = "", suffix = "";
			string tokenValue = token.Value;
			if (!token.IsNonScriptText && token.TokenType == TokenType.StringLiteral)
				tokenValue = "\"" + tokenValue + "\"";
			int start2 = start + code.Length + tokenValue.Length;
			string code2 = source.Substring(start2, Math.Min(150, source.Length - start2));

			if (start > 0) prefix = "...";
			if (source.Length - start2 > 150) suffix = "...";

			string names = "";
			if (state == null)
				names = identifier.DescriptiveName;
			else
			{
				while (state.ScriptIdentifierStack.Count > 0)
				{
					if (names.Length > 0)
						names = " &gt; " + names;
					names = state.ScriptIdentifierStack.Pop().DescriptiveName + names;
				}
			}
			return "<style>body{font-family:verdana;font-size:8pt;}</style>"
					+ "<body><h2>Couldn't understand part of the script :(</h2>"
					+ "<strong style=\"color:red\">" + HttpUtility.HtmlEncode(message) + "</strong><br/>"
					+ "The script I was processing was: <strong>" + names + "</strong><br />"
					+ "The error occurred at position " + token.Position + " in the script, at a section that looks like this:<br/><br/>"
					+ "<div style=\"padding:10px;background-color:#ffd;border:1px dotted #cca;\">"
					+ "<pre style=\"margin:0\">" + prefix + HttpUtility.HtmlEncode(code)
					+ "<span style=\"color:red\">" + HttpUtility.HtmlEncode(tokenValue) + "</span>"
					+ HttpUtility.HtmlEncode(code2) + suffix + "</pre></div></body>";
		}

		private Dictionary<string, SprocketScript> sectionOverrides = new Dictionary<string, SprocketScript>();
		public void OverrideSection(string sectionName, SprocketScript script)
		{
			sectionOverrides[sectionName] = script;
		}

		public void Execute(Stream stream)
		{
			using (StreamWriter writer = new StreamWriter(stream))
				writer.Write(Execute());
		}

		public string Execute()
		{
			MemoryStream stream = new MemoryStream();
			ExecutionState state = new ExecutionState(stream);
			state.ScriptIdentifierStack.Push(identifier);
			state.ExecutingScript.Push(this);
			state.SectionOverrides = sectionOverrides;
			try
			{
				instruction.Execute(state);
				stream.Seek(0, SeekOrigin.Begin);
				using (StreamReader reader = new StreamReader(stream))
					return reader.ReadToEnd();
			}
			catch (InstructionExecutionException ex)
			{
				return GetErrorHTML(ex.Message, ex.Token, state);
			}
		}

		public string ExecuteToResolveExpression(ExecutionState baseState)
		{
			if (baseState.BaseExecutionState.ScriptIdentifierStack.Contains(identifier))
				throw new InstructionExecutionException("Script \"" + identifier.DescriptiveName + "\" aborted to prevent infinite recursion", baseState.SourceToken);

			MemoryStream stream = new MemoryStream();
			ExecutionState state = new ExecutionState(stream, baseState);
			baseState.ScriptIdentifierStack.Push(identifier);
			baseState.ExecutingScript.Push(this);
			state.SectionOverrides = sectionOverrides;

			instruction.Execute(state);

			baseState.ScriptIdentifierStack.Pop();
			baseState.ExecutingScript.Pop();

			stream.Seek(0, SeekOrigin.Begin);
			using (StreamReader reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}

		/// <summary>
		/// Executes the script as a branch of execution of another script, maintaining the
		/// existing execution state. This is used exclusively for allowing certain named
		/// script sections (InstructionList objects) to execute a secondary script instead
		/// of their standard child instructions.
		/// </summary>
		/// <param name="state">The state in which to continue execution</param>
		internal void ExecuteInParentContext(ExecutionState state)
		{
			if (state.BaseExecutionState.ScriptIdentifierStack.Contains(identifier))
				throw new InstructionExecutionException("Script \"" + identifier.DescriptiveName + "\" aborted to prevent infinite recursion", state.SourceToken);

			state.BaseExecutionState.ScriptIdentifierStack.Push(identifier);
			state.BaseExecutionState.ExecutingScript.Push(this);
			
			Dictionary<string, SprocketScript> preservedOverrides = state.SectionOverrides;
			state.SectionOverrides = sectionOverrides;
			instruction.Execute(state);
			state.SectionOverrides = preservedOverrides;

			state.BaseExecutionState.ScriptIdentifierStack.Pop();
			state.BaseExecutionState.ExecutingScript.Pop();
		}
	}

	/// <summary>
	/// This class maintains state information during script execution. Often there will be a case where a script
	/// causes another script to be executed. This can happen two ways. (a) The script can pass program flow to
	/// the secondary script by calling that script's Execute method and passing this ExecutionState object so that
	/// the secondary script will continue to write to the same stream and perform correct recursion checking.
	/// (b) The script can execute the secondary script independently in order to resolve the value of an expression.
	/// When this happens, the secondary script will need to use its own internal ExecutionState object as it will be
	/// writing a new temporary stream that will be used as the expression value in the primary script. The problem
	/// involved with such a scenario is that the child script can then cause infinite recursion as it has its own
	/// ScriptNameStack, and thus entries in the primary stack won't be considered. To avoid this, when calling a
	/// secondary script in order to resolve an expression[to do]
	/// </summary>
	public class ExecutionState
	{
		private Stack<ScriptRecursionIdentifier> scriptIdentifierStack = new Stack<ScriptRecursionIdentifier>();
		/// <summary>
		/// Used for recursion checking. Scripts are not passed parameters. They act as independent output generators
		/// and thus if one either directly or indirectly calls itself, an infinite recursion loop is almost guaranteed.
		/// Use this parameter to check whether or not the script is already on the stack and thus likely to cause
		/// infinite recursion. Note: ALWAYS call this in the context of BaseExecutionState, never directly.
		/// </summary>
		public Stack<ScriptRecursionIdentifier> ScriptIdentifierStack
		{
			get { return scriptIdentifierStack; }
		}

		public class ScriptRecursionIdentifier
		{
			private string descriptiveName, identificationString;

			public string DescriptiveName
			{
				get { return descriptiveName; }
			}

			public string IdentificationString
			{
				get { return identificationString; }
			}

			public ScriptRecursionIdentifier(string descriptiveName, string identificationString)
			{
				this.descriptiveName = descriptiveName;
				this.identificationString = identificationString;
			}

			public override bool Equals(object obj)
			{
				return obj.ToString() == identificationString;
			}
		}

		private Stack<SprocketScript> executingScript = new Stack<SprocketScript>();
		/// <summary>
		/// Not used for recursion checking. This used simply to store which script is currently executing so that
		/// when an error is thrown, the exception handler can get the erroneous source code from the correct object.
		/// Note: ALWAYS call this in the context of BaseExecutionState, never directly.
		/// </summary>
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
			baseExecutionState = this;
		}

		public ExecutionState(Stream stream, ExecutionState state)
		{
			output = new StreamWriter(stream);
			output.AutoFlush = true;
			baseExecutionState = state;
		}

		private Dictionary<string, SprocketScript> sectionOverrides = new Dictionary<string, SprocketScript>();
		public Dictionary<string, SprocketScript> SectionOverrides
		{
			get { return sectionOverrides; }
			set { sectionOverrides = value; }
		}

		private ExecutionState baseExecutionState;
		public ExecutionState BaseExecutionState
		{
			get { return baseExecutionState; }
		}

		private Token sourceToken = null;
		public Token SourceToken
		{
			get { return sourceToken; }
			set { sourceToken = value; }
		}
	}
}
