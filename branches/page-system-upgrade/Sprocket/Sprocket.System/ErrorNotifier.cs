#define ENABLE_PROGRAM_FLOW_LOG

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;

using Sprocket.Web;
using Sprocket.Mail;

namespace Sprocket
{
	public static class ErrorNotifier
	{
		public static void Process(Exception e)
		{
			bool sendEmail = SprocketSettings.GetBooleanValue("SendErrorEmail");
			HttpRequest Request = HttpContext.Current.Request;

			string strauth = "";
			string form = "";
			string headers = "";
			string fulltext = "";
			string path = "";
			string divider = "----------------------------------------------------------" + Environment.NewLine;
			string cookies = "";
			bool isAjax = false;
			try
			{
				isAjax = AjaxRequestHandler.IsAjaxRequest;
			}
			catch
			{ }
			try
			{
				if (WebAuthentication.IsLoggedIn)
					strauth = "Username: " + (WebAuthentication.Instance.CurrentUsername ?? "[null]") + Environment.NewLine;
				else
					strauth = "The user was not signed in at the time." + Environment.NewLine;
				strauth += "Host address/IP: " + Request.UserHostAddress + " / " + Request.UserHostName + Environment.NewLine;
			}
			catch (Exception ex)
			{
				strauth = "ERROR EVALUATING AUTHENTICATION STATE: " + ex + "\r\n\r\n";
			}
			try
			{
				foreach (string key in HttpContext.Current.Request.Headers)
					if (key == "Cookie")
					{
						cookies = "Cookie Data:" + Environment.NewLine;
						foreach (string k in Request.Headers[key].Split(';'))
						{
							string[] arr = k.Split('=');
							string val = arr.Length > 0 ? k.Substring(arr[0].Length + 1) : "";
							cookies += "[ " + arr[0] + " ]" + Environment.NewLine + val + Environment.NewLine;
						}
					}
					else
						headers += key + ": " + HttpContext.Current.Request.Headers[key] + Environment.NewLine;
			}
			catch (Exception ex)
			{
				headers = "ERROR EVALUATING HEADERS: " + ex + "\r\n\r\n";
			}
			try
			{
				if (Request.Form.Count > 0)
				{
					form += Environment.NewLine + "FORM POST DATA:" + Environment.NewLine;
					foreach (string key in Request.Form.AllKeys)
						form += key + ": " + Request.Form[key] + Environment.NewLine;
				}
				else
					form = "There was no form POST data (i.e. this was not a submitted form)." + Environment.NewLine;
			}
			catch (Exception ex)
			{
				headers = "ERROR EVALUATING FORM POST DATA: " + ex + "\r\n\r\n";
			}

			try
			{
				path = "The SprocketPath for the request was: " + SprocketPath.Value + Environment.NewLine;
				path += "The full URL was: " + Request.RawUrl + Environment.NewLine;
			}
			catch (Exception ex)
			{
				path = "ERROR EVALUATING SPROCKET PATH: " + ex + "\r\n\r\n";
			}
			fulltext = "An exception was thrown by the application." + Environment.NewLine
				+ strauth + divider
				+ path + divider
				+ form + divider
				+ cookies + divider
				+ "The HTTP Headers were:" + Environment.NewLine
				+ headers + divider
				+ "Program flow log:" + Environment.NewLine
				+ ProgramFlowLog.Value + divider
				+ "The exception thrown was:" + Environment.NewLine
				+ e.ToString();
			if (e.InnerException != null)
				fulltext += Environment.NewLine + divider + "INNER EXCEPTION:" + Environment.NewLine + e.InnerException;
			if (sendEmail)
			{
				try
				{
					EmailHandler.Send(EmailHandler.AdminEmailAddress, EmailHandler.NullEmailAddress, "APPLICATION ERROR", fulltext, false);
				}
				catch (Exception ex)
				{
					fulltext += "ERROR SENDING EMAIL: " + ex.ToString();
				}
			}
			try
			{
				LogFile.Append(string.Format("error-{0:yyyy-MM-dd-HH-mm-ss-fff}.txt", DateTime.UtcNow), fulltext);
			}
			catch
			{ }
		}
	}

	public class ProgramFlowLog : IDisposable
	{
		public ProgramFlowLog(params object[] prmValues)
		{
#if ENABLE_PROGRAM_FLOW_LOG
			Indent++;
			StackFrame frame = new StackFrame(1, false);
			MethodBase method = frame.GetMethod();
			StringBuilder sb = new StringBuilder();
			sb
				.Append(method.DeclaringType.FullName)
				.Append(".")
				.Append(method.Name)
				.Append("(");
			if (prmValues.Length > 0)
			{
				sb.Append((prmValues[0] ?? "{null}").ToString());
				for (int i = 0; i < prmValues.Length; i++)
					sb.Append((prmValues[i] ?? "{null}").ToString());
			}
			sb.Append(")");
			AddLogMessage(sb.ToString());
#endif
		}

		public void Dispose()
		{
#if ENABLE_PROGRAM_FLOW_LOG
			Indent--;
#endif
		}

		private void AddLogMessage(string str)
		{
#if ENABLE_PROGRAM_FLOW_LOG
			StringBuilder sb = CurrentRequest.Value["ProgramFlowLog.StackLogMessages"] as StringBuilder;
			if (sb == null)
				CurrentRequest.Value["ProgramFlowLog.StackLogMessages"] = sb = new StringBuilder();
			for (int i = 0; i < (Indent * 4) - 4; i++)
				sb.Append(' ');
			sb.AppendLine(str);
#endif
		}

		private int Indent
		{
			get
			{
				int? n = CurrentRequest.Value["ProgramFlowLog.Indent"] as int?;
				if (!n.HasValue)
					CurrentRequest.Value["ProgramFlowLog.Indent"] = n = 0;
				return n.Value;
			}
			set
			{
				CurrentRequest.Value["ProgramFlowLog.Indent"] = value;
			}
		}

		public static string Value
		{
			get
			{
#if ENABLE_PROGRAM_FLOW_LOG
				return (CurrentRequest.Value["ProgramFlowLog.StackLogMessages"] as StringBuilder ?? new StringBuilder()).ToString();
#else
				return "[ProgramFlowLog Disabled]";
#endif
			}
		}
	}
}