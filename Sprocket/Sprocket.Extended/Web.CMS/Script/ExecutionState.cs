using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;


namespace Sprocket.Web.CMS.Script
{
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

		private string errorHTML = null;
		public bool ErrorOccurred
		{
			get { return errorHTML != null; }
		}
		public string ErrorHTML
		{
			get { return errorHTML; }
			internal set { errorHTML = value; }
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

			public override string ToString()
			{
				return identificationString;
			}
			public override int GetHashCode()
			{
				return identificationString.GetHashCode();
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

		private Stack<StreamWriter> output = new Stack<StreamWriter>();
		public StreamWriter Output
		{
			get { return output.Peek(); }
		}

		/// <summary>
		/// causes output to start building in an isolated area that can be merged into the main output at a later point
		/// </summary>
		public void BranchOutput()
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			output.Push(writer);
		}

		/// <summary>
		/// merges the current output branch into the underlying output stream, which it then returns control to, closing the current branch in the process.
		/// </summary>
		public void MergeBranch()
		{
			if (ErrorOccurred)
			{
				output.Pop().Dispose();
				output.Peek().Write(ErrorHTML);
				return;
			}
			StreamWriter writer = output.Pop();
			writer.Flush();
			writer.BaseStream.Seek(0, SeekOrigin.Begin);
			StreamReader reader = new StreamReader(writer.BaseStream);
			output.Peek().Write(reader.Read());
			writer.Close();
		}

		/// <summary>
		/// reads and returns the contents of the current branch without merging it into the underlying output stream
		/// </summary>
		public string ReadAndRemoveBranch()
		{
			if (ErrorOccurred)
			{
				output.Pop().Dispose();
				return ErrorHTML;
			}
			StreamWriter writer = output.Pop();
			writer.Flush();
			writer.BaseStream.Seek(0, SeekOrigin.Begin);
			StreamReader reader = new StreamReader(writer.BaseStream);
			string text = reader.ReadToEnd();
			writer.Close();
			return text;
		}

		/// <summary>
		/// removes the contents of the existing output branch from the stack and discards it without adding it to the underlying output stream
		/// </summary>
		public void CancelBranch()
		{
			output.Pop();
		}

		public ExecutionState(Stream stream)
		{
			StreamWriter writer = new StreamWriter(stream);
			writer.AutoFlush = true;
			output.Push(writer);
		}

		public ExecutionState(params KeyValuePair<string, object>[] preloadedVariables)
		{
			foreach (KeyValuePair<string, object> kvp in preloadedVariables)
				variables.Add(kvp.Key, kvp.Value);
			BranchOutput();
		}

		public ExecutionState()
		{
			BranchOutput();
		}

		private Token sourceToken = null;
		public Token SourceToken
		{
			get { return sourceToken; }
			set { sourceToken = value; }
		}

		private Dictionary<string, object> variables = new Dictionary<string, object>();
		public Dictionary<string, object> Variables
		{
			get { return variables; }
		}

		public bool HasVariable(string variableName)
		{
			return Variables.ContainsKey(variableName);
		}

		public object GetVariable(string variableName)
		{
			return Variables[variableName];
		}
	}
}
