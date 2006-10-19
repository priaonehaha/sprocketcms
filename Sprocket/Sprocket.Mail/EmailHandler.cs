using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Mail;

using Sprocket;
using Sprocket.Utility;

namespace Sprocket.Mail
{
	[ModuleDescription("Handles sending of emails and notification of delivery results")]
	[ModuleTitle("Email Handler")]
	public class EmailHandler : ISprocketModule
	{
		public event InterruptableEventHandler<MailMessage> OnSendingEmail;
		public NotificationEventHandler<MailSendResult> OnEmailDeliverySuccess;
		public NotificationEventHandler<MailSendResult> OnEmailDeliveryFailed;

		public static EmailHandler Instance
		{
			get { return (EmailHandler)Core.Instance[typeof(EmailHandler)].Module; }
		}

		public static void Send(MailMessage msg)
		{
			Instance.Send(msg, false, null);
		}

		public static void SendAsync(MailMessage msg, string callbackModuleRegCode)
		{
			Instance.Send(msg, true, callbackModuleRegCode);
		}

		public static void Send(MailAddress to, MailAddress from, string subject, string body, bool isHTML)
		{
			MailMessage msg = new MailMessage(from, to);
			msg.Subject = subject;
			msg.Body = body;
			msg.IsBodyHtml = isHTML;
			Instance.Send(msg, false, null);
		}

		public static void SendAsync(MailAddress to, MailAddress from, string subject, string body, bool isHTML, string callbackModuleRegCode)
		{
			MailMessage msg = new MailMessage(from, to);
			msg.Subject = subject;
			msg.Body = body;
			msg.IsBodyHtml = isHTML;
			Instance.Send(msg, true, callbackModuleRegCode);
		}

		public static void SendAsync(MailMessage msg)
		{
			Instance.Send(msg, true, null);
		}

		private void Send(MailMessage msg, bool async, string moduleCode)
		{
			if (OnSendingEmail != null)
			{
				Result r = new Result();
				OnSendingEmail(msg, r);
				if (!r.Succeeded && OnEmailDeliveryFailed != null)
					OnEmailDeliveryFailed(new MailSendResult(msg, moduleCode, r.Message));
			}
			SmtpClient client = GetClient();
			if (async)
			{
				client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
				client.SendAsync(msg, new MailSendResult(msg, moduleCode, null));
			}
			else
			{
				try
				{
					client.Send(msg);
				}
				catch (Exception ex)
				{
					if (OnEmailDeliveryFailed != null)
						OnEmailDeliveryFailed(new MailSendResult(msg, moduleCode, ex.Message));
					return;
				}
				if (OnEmailDeliverySuccess != null)
					OnEmailDeliverySuccess(new MailSendResult(msg, moduleCode, null));
			}
		}

		public static MailAddress NullEmailAddress
		{
			get
			{
				return new MailAddress(
					SprocketSettings.GetValue("NullEmailAddress"),
					SprocketSettings.GetValue("NullEmailAddressName"));
			}
		}

		public static MailAddress AdminEmailAddress
		{
			get
			{
				return new MailAddress(
					SprocketSettings.GetValue("AdminEmailAddress"),
					SprocketSettings.GetValue("AdminEmailAddressName"));
			}
		}

		void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (e.Error != null && OnEmailDeliveryFailed != null)
			{
				MailSendResult r = (MailSendResult)e.UserState;
				r.ErrorMessage = e.Error.Message;
				OnEmailDeliveryFailed(r);
			}
			else if (e.Error == null && OnEmailDeliverySuccess != null)
				OnEmailDeliverySuccess((MailSendResult)e.UserState);
		}

		private SmtpClient GetClient()
		{
			string mailserver = SprocketSettings.GetValue("MailServer");
			int port;
			int.TryParse(SprocketSettings.GetValue("MailServerPort"), out port);
			string useAuthentication = SprocketSettings.GetValue("MailServerAuthentication");
			string authUsername = SprocketSettings.GetValue("MailServerUsername");
			string authPassword = SprocketSettings.GetValue("MailServerPassword");

			SmtpClient client = new SmtpClient();
			if (mailserver != null)
				client.Host = mailserver;
			else
				client.Host = "localhost";
			if (port > 0) client.Port = port;
			if (useAuthentication != null && authUsername != null && authPassword != null)
				if (Utilities.MatchesAny(useAuthentication.ToLower(), "true", "yes", "1"))
					client.Credentials = new NetworkCredential(authUsername, authPassword);

			return client;
		}

		public static bool IsValidEmailAddress(string email)
		{
			try
			{
				MailAddress m = new MailAddress(email);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
		}

		#endregion
	}

	public class MailSendResult
	{
		private MailMessage mailMessage;
		private string moduleNamespace;
		private string errorMessage;

		public string ErrorMessage
		{
			get { return errorMessage; }
			set { errorMessage = value; }
		}

		public MailMessage MailMessage
		{
			get { return mailMessage; }
		}

		public string ModuleNamespace
		{
			get { return moduleNamespace; }
		}

		public MailSendResult(MailMessage msg, string moduleRegCode, string errorMsg)
		{
			mailMessage = msg;
			moduleNamespace = moduleRegCode;
			errorMessage = errorMsg;
		}
	}
}
