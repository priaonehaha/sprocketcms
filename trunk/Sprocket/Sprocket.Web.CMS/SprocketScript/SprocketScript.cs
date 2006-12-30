using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using Sprocket.Web.CMS.SprocketScript.Parser;

namespace Sprocket.Web.CMS.SprocketScript
{
	public sealed class SprocketScript
	{
		private IInstruction instruction;
		private string source;

		public SprocketScript(string source)
		{
			this.source = source;
			List<Token> tokens = Tokeniser.Extract(source);
			try
			{
				instruction = TokenParser.BuildInstruction(tokens);
			}
			catch (TokenParserException ex)
			{
				tokens.Clear();
				tokens.Add(new Token(GetErrorHTML(ex.Message, ex.Token), TokenType.StringLiteral, 0));
				instruction = new ShowInstruction();
				int n = 0;
				instruction.Build(tokens, ref n);
			}
		}

		string GetErrorHTML(string message, Token token)
		{
			int start = Math.Max(token.Position - 150, 0);
			string code = source.Substring(start, token.Position - start);
			string prefix = "", suffix = "";
			int start2 = start + code.Length + token.Value.Length;
			string code2 = source.Substring(start2, Math.Min(150, source.Length - start2));

			if (start > 0) prefix = "...";
			if (source.Length - start2 > 150) suffix = "...";

			return "<style>body{font-family:verdana;font-size:8pt;}</style>"
					+ "<body><h2>Couldn't understand part of the script :(</h2>"
					+ "<strong style=\"color:red\">" + HttpUtility.HtmlEncode(message) + "</strong><br/>"
					+ "The error occurred at position " + token.Position + " in the script, at a section that looks like this:<br/><br/>"
					+ "<div style=\"padding:10px;background-color:#ffd;border:1px dotted #cca;\">"
					+ "<pre style=\"margin:0\">" + prefix + HttpUtility.HtmlEncode(code)
					+ "<span style=\"color:red\">" + HttpUtility.HtmlEncode(token.Value) + "</span>"
					+ HttpUtility.HtmlEncode(code2) + suffix + "</pre></div></body>";
		}

		public void Execute(Stream stream)
		{
			using(StreamWriter writer = new StreamWriter(stream))
				writer.Write(Execute());
		}

		public string Execute()
		{
			MemoryStream stream = new MemoryStream();
			ExecutionState state = new ExecutionState(stream);
			try
			{
				instruction.Execute(state);
				stream.Seek(0, SeekOrigin.Begin);
				using (StreamReader reader = new StreamReader(stream))
					return reader.ReadToEnd();
			}
			catch (InstructionExecutionException ex)
			{
				return GetErrorHTML(ex.Message, ex.Token);
			}
		}
	}
}
