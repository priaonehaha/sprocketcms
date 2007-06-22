using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Sprocket.Web
{
	public static class SprocketPath
	{
		public static string Value
		{
			get { return (string)CurrentRequest.Value["Sprocket.Web.SprocketPath.Value"]; }
			internal set { CurrentRequest.Value["Sprocket.Web.SprocketPath.Value"] = value; }
		}

		public static string RawURL
		{
			get { return (string)CurrentRequest.Value["Sprocket.Web.SprocketPath.RawURL"]; }
			internal set { CurrentRequest.Value["Sprocket.Web.SprocketPath.RawURL"] = value; }
		}

		public static string RawQueryString
		{
			get { return (string)CurrentRequest.Value["Sprocket.Web.SprocketPath.RawQueryString"]; }
			internal set { CurrentRequest.Value["Sprocket.Web.SprocketPath.RawQueryString"] = value; }
		}

		public static string[] Sections
		{
			get { return (string[])CurrentRequest.Value["Sprocket.Web.SprocketPath.PathSections"]; }
			internal set { CurrentRequest.Value["Sprocket.Web.SprocketPath.PathSections"] = value; }
		}

		public static string Physical
		{
			get
			{
				string s = CurrentRequest.Value["SprocketPath.Physical"] as string;
				if (s == null)
				{
					s = WebUtility.MapPath(Value);
					CurrentRequest.Value["SprocketPath.Physical"] = s;
				}
				return s;
			}
		}

		internal static void Parse(Uri uri)
		{
			HttpRequest Request = HttpContext.Current.Request;

			RawQueryString = uri.Query.Length > 0 ? uri.Query.Substring(1) : "";
			Value = HttpUtility.UrlDecode(uri.AbsolutePath).ToLower().Substring(Request.ApplicationPath.Length).Trim('/');
			Sections = Value.Split('/');
			RawURL = uri.OriginalString;
		}

		internal static void Parse(string rawURL)
		{
			Parse(new Uri(rawURL));
		}

		public static bool IsPathDescendentOf(string descendentPath, string parentPath)
		{
			if (parentPath.Length + 1 >= descendentPath.Length)
				return false;
			if (descendentPath.Substring(parentPath.Length, 1) != "/")
				return false;
			return descendentPath.StartsWith(parentPath);
		}

		public static bool IsCurrentPathDescendentOf(string parentPath)
		{
			return IsPathDescendentOf(Value, parentPath);
		}

		public static string GetDescendentPath(string descendentPath, string parentPath)
		{
			if (parentPath.Length + 1 >= descendentPath.Length) // don't allow for just a trailing slash
				return "";
			if (descendentPath.Substring(parentPath.Length, 1) != "/")
				return "";
			return descendentPath.Substring(0, parentPath.Length + 1);
		}

		public static string GetDescendentPath(string parentPath)
		{
			string descendentPath = Value;
			if (parentPath.Length + 1 >= descendentPath.Length) // don't allow for just a trailing slash
				return "";
			if (descendentPath.Substring(parentPath.Length, 1) != "/")
				return "";
			return descendentPath.Substring(parentPath.Length + 1);
		}

		public static string ExtractSprocketPath(string fullURL)
		{
			return HttpContext.Current.Request.Path.ToLower().Remove(0, HttpContext.Current.Request.ApplicationPath.Length).Trim('/');
		}
	}
}
