using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

using Sprocket.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Security
{
	public partial class WebSecurity
	{
		internal void RenderCAPTCHAImage()
		{
			string chars = DecryptCAPTCHAKey(SprocketPath.Sections[1]);

			Bitmap bmp = new Bitmap(90, 25);
			Graphics gfx = Graphics.FromImage(bmp);
			gfx.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
			StringFormat fmt = new StringFormat();
			fmt.LineAlignment = StringAlignment.Center;
			fmt.Alignment = StringAlignment.Center;
			fmt.Trimming = StringTrimming.None;
			fmt.FormatFlags = StringFormatFlags.NoWrap;
			gfx.DrawString(chars, new Font("Impact", 16f, FontStyle.Italic | FontStyle.Strikeout, GraphicsUnit.Point), Brushes.Black,
				new Rectangle(0,0,bmp.Width,bmp.Height), fmt);
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			ImageCodecInfo encoder = null;
			for (int i = 0; i < encoders.Length; i++)
				if (encoders[i].MimeType == "image/jpeg")
				{
					encoder = encoders[i];
					break;
				}
			if (encoder == null)
				throw new Exception("Can't create a thumbnail because no JPEG encoder exists.");
			EncoderParameters prms = new EncoderParameters(1);

			// set the jpeg quality
			prms.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
			HttpContext.Current.Response.ContentType = "image/jpeg";
			MemoryStream stream = new MemoryStream();
			bmp.Save(stream, encoder, prms);
			HttpContext.Current.Response.BinaryWrite(stream.ToArray());
			HttpContext.Current.Response.End();
		}

		public static string EncryptNewCAPTCHAKey()
		{
			Random r = new Random();
			int n1 = Convert.ToInt32('a');
			int n2 = Convert.ToInt32('z') + 1;
			string str = "";
			while (str == "" || Instance.expiredCaptchaKeys.Contains(str))
			{
				str = "";
				while (str.Length < 6)
					str += Convert.ToChar(r.Next(n1, n2));
			}
			string key = SprocketSettings.GetValue("EncryptionKeyWord");
			string vector = HttpContext.Current.Request.UserHostAddress;
			vector = vector.Substring(0, vector.LastIndexOf('.'));
			return StringUtilities.HexStringFromBytes(Crypto.RC2Encrypt(str.ToUpper(), key, vector));
		}

		internal static string DecryptCAPTCHAKey(string encryptedCaptcha)
		{
			string key = SprocketSettings.GetValue("EncryptionKeyWord");
			string vector = HttpContext.Current.Request.UserHostAddress;
			vector = vector.Substring(0, vector.LastIndexOf('.'));

			return Crypto.RC2Decrypt(StringUtilities.BytesFromHexString(encryptedCaptcha), key, vector);
		}

		private Queue<string> expiredCaptchaKeys = new Queue<string>();
		internal static void ExpireCAPTCHAKey(string key)
		{
			if (Instance.expiredCaptchaKeys.Count == 10000)
				Instance.expiredCaptchaKeys.Dequeue();
			Instance.expiredCaptchaKeys.Enqueue(key);
		}

		internal static bool IsCAPTCHAKeyExpired(string key)
		{
			return Instance.expiredCaptchaKeys.Contains(key);
		}

		public static bool ValidateCAPTCHAInput(string input, string encryptedCaptcha, bool expireKey)
		{
			string chars = DecryptCAPTCHAKey(encryptedCaptcha);
			if (IsCAPTCHAKeyExpired(chars))
				return false;
			if(expireKey)
				ExpireCAPTCHAKey(chars);
			return chars == input.ToUpper();
		}
	}

	public class CaptchaKeyExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return WebSecurity.EncryptNewCAPTCHAKey();
		}
	}

	public class CaptchaKeyExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "new_captcha_key"; }
		}

		public IExpression Create()
		{
			return new CaptchaKeyExpression();
		}
	}
}
