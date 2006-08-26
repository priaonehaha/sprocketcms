using System;
using System.Web;
using System.Collections.Generic;
using System.IO;

using Sprocket.SystemBase;
using Sprocket.Utility;
using Sprocket;

namespace Sprocket.Web
{
	/// <summary>
	/// The WebEvents class implements the ISprocketModule interface and serves as the interface between
	/// Sprocket and the ASP.Net pipeline. It hooks into several major ASP.Net pipeline events and then
	/// exposes its own events for other Sprocket modules to use. Specifically, it encapsulates the
	/// ability for applications to use virtual paths that don't actually have to correspond to physical
	/// locations on the web server, thus simplifying directory structures and improving the versatility
	/// of the web address in determining what content to serve. This module is interdependent with the
	/// WebInitialiserHttpModule class, which implements the IHttpModule interface required for Sprocket
	/// to hook into the ASP.Net pipeline.
	/// </summary>
	[ModuleDependency("SprocketSettings")]
	[ModuleDependency("SystemEvents")]
	public class WebEvents : ISprocketModule
	{
		public delegate void HttpApplicationEventHandler(HttpApplication app);
		public delegate void HttpApplicationCancellableEventHandler(HttpApplication app, HandleFlag handled);
		public delegate void ApplicationErrorEventHandler(HttpApplication app, Exception e);
		public delegate void RequestedPathEventHandler(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled);

		/// <summary>
		/// Pretty much the first event exposed by Sprocket during a request. This should generally
		/// not be used for rendering data to the screen except in special cases, as no sessions,
		/// cookies or other request-state data has been loaded yet. Standard things to do here are
		/// house-keeping chores or other behind-the-scenes work that should occur at the start of
		/// a request.
		/// </summary>
		public event HttpApplicationCancellableEventHandler OnBeginHttpRequest;
		
		/// <summary>
		/// Fires right before Sprocket finishes handling the request and releases it back to ASP.Net.
		/// </summary>
		public event HttpApplicationEventHandler OnEndHttpRequest;
		
		/// <summary>
		/// Fires when cookies, sessions, etc have been loaded and made available. This is called
		/// before Sprocket does any standard path processing, and should generally be used with
		/// the same caution as the OnBeginHttpRequest event, albeit with the knowledge that state
		/// information is now available.
		/// </summary>
		public event HttpApplicationEventHandler OnRequestStateLoaded;

		/// <summary>
		/// Allows an exception thrown somewhere in Sprocket's pipeline to be caught and handled
		/// gracefully before we get a standard ASP.Net error page.
		/// </summary>
		public event ApplicationErrorEventHandler OnApplicationError;

		/// <summary>
		/// Sprocket processes the path information into nice little easy-to-read containers and
		/// variables ready for you to process requests with. It also handles the nasty business
		/// of trying to work out if we're operating out of a Virtual Directory or not and hides
		/// that bothersome detail from you. This event should be used for processing actual
		/// website requests, using the path data passed in the event handler. You can set the
		/// "handled" flag to let Sprocket know that you handled the request and stop it from
		/// throwing a 404 error or anything like that, and also to let other modules know it's
		/// been handled in case they want a piece of the action as well.
		/// </summary>
		public event RequestedPathEventHandler OnLoadRequestedPath;

		/// <summary>
		/// This event allows you to "have the last word" and do your own custom error page handling
		/// before a standard ASP.Net 404 page is served. Naturally, use the "handled" flag to let
		/// Sprocket know you handled the 404 page and prevent it from serving the standard 404 page.
		/// </summary>
		public event RequestedPathEventHandler OnPathNotFound;

		/// <summary>
		/// This is the very first point where Sprocket interrupts the ASP.Net HTTP pipeline
		/// and allows itself to start handling requests. Note that this is way before the 
		/// standard ASP.Net page framework would kick in. At this point state information like
		/// cookies and sessions have not yet been loaded.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void FireBeginRequest(object sender, EventArgs e)
		{
			HandleFlag handled = new HandleFlag();

			if(OnBeginHttpRequest != null)
				OnBeginHttpRequest((HttpApplication)sender, handled);

			if (handled.Handled)
			{
				HttpContext.Current.Response.End();
				return;
			}

			// The SprocketSettings module is one of the modules that handles the OnBeginHttpRequest
			// event. It lets each module check for any .config file errors (or other settings errors)
			// and report them back here. If we get to this point and at least one module has reported
			// a settings error, we show Sprocket's critical error page which has a nice list of
			// error messages that the user can try to rectify.
			if (((SprocketSettings)SystemCore.Instance["SprocketSettings"]).ErrorList.HasCriticalError)
			{
				ShowErrorPage();
				return;
			}
		}

		/// <summary>
		/// Sprocket calls this method in response to ASP.Net's EndRequest event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void FireEndRequest(object sender, EventArgs e)
		{
			if(OnEndHttpRequest != null)
				OnEndHttpRequest((HttpApplication)sender);

			// seeing as this is the end of the line for a Sprocket request, let the system events
			// module know so that other modules can clean up if necessary, close database connections
			// and anything else relevant.
			((SystemEvents)SystemCore.Instance["SystemEvents"]).NotifySessionEnding();
		}

		private string sprocketPath = null;
		public static string SprocketPath
		{
			get { return Instance.sprocketPath; }
		}

		private string[] sprocketPathSections = null;
		public static string[] SprocketPathSections
		{
			get { return Instance.sprocketPathSections; }
		}

		/// <summary>
		/// Sprocket calls this method in response to ASP.Net's AcquireRequestState event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void FireAcquireRequestState(object sender, EventArgs e)
		{
			if (OnRequestStateLoaded != null) // as always, let the other modules know where we are...
				OnRequestStateLoaded((HttpApplication)sender);

			HttpContext pg = HttpContext.Current;

			// The SprocketPath refers to the bit after the application base path and before the
			// querystring, minus any leading and trailing forward-slashes. (/) For example if the
			// full URL is "http://www.sprocketcms.com/myapp/admin/users/?edit" and the subdirectory
			// "myapp" is a virtual directory (IIS application) then the SprocketPath would be
			// "admin/users".
			string sprocketPath = null;
			string appPath = pg.Request.Path.ToLower();

			// check to see if there's a trailing slash and if there isn't, redirect to stick a trailing
			// slash onto the path. This is to keep pathing consistent because otherwise relative paths
			// (such as to images and css files) aren't pathed as expected. We DON'T do this if a form
			// has been posted however, because otherwise we lose the contents of the posted form. It is
			// assumed that if you forget to post to a path with a trailing slash, that once you finish
			// processing the form that you'll redirect off to a secondary page anyway, which means
			// sticking a slash on the end of this URL is unnecessary anyway.
			if (!appPath.EndsWith("/") && !appPath.Contains(".") && HttpContext.Current.Request.Form.Count == 0)
			{
				pg.Response.Redirect(appPath + "/");
				pg.Response.End();
				return;
			}

			// changes (e.g.) "http://www.sprocketcms.com/myapp/admin/users/?edit" into "admin/users"
			sprocketPath = appPath.Remove(0, pg.Request.ApplicationPath.Length).Trim('/');

			// split up the path sections to make things even easier for request event handlers
			string[] pathSections = sprocketPath.Split('/');

			// this is our flag so that request event handlers can let us know if they handled this request.
			HandleFlag flag = new HandleFlag();

			if (OnLoadRequestedPath != null)
			{
				OnLoadRequestedPath((HttpApplication)sender, sprocketPath, pathSections, flag);
				if (flag.Handled)
				{
					// stop the browser from caching the page
					// HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);

					// if one of the modules handled the request event, then we can stop
					// doing stuff now. The OnEndRequest event will still be called though.
					pg.Response.End();
					return;
				}
			}

			// if we've reached this point and none of our modules have volunteered to handle
			// the request, we can check to see if the requested path actually exists (gasp!)
			// and if so, serve up that file! This is handy if we insist on using the Standard
			// ASP.Net Page framework (yuck) or want to serve up other things like plain html
			// files.
			if (!flag.Handled && File.Exists(pg.Request.PhysicalPath))
			{
				HttpContext.Current.RewritePath(pg.Request.Path);
				return;
			}

			// at this point we know that no file matching the exists, so we can check to see
			// if a directory of the specified name exists. If it does, we can see if there are
			// any default pages inside the folder that should execute. This requires the a key
			// to be configured for appSettings in the Web.config file:
			// <add key="DefaultPageFilenames" value="default.aspx,default.asp,default.htm,index.htm" />
			if (Directory.Exists(pg.Request.PhysicalPath))
			{
				string dpgstr = SprocketSettings.GetValue("DefaultPageFilenames");
				if (dpgstr != null)
				{
					string[] pgarr = dpgstr.Split(',');
					foreach (string pgname in pgarr)
					{
						string pgpath = "/" + pg.Request.Path.Trim('/') + "/" + pgname;
						string physpath = pg.Request.PhysicalPath + "\\" + pgname;
						if (File.Exists(physpath))
						{
							HttpContext.Current.Response.Redirect(pgpath);
							return;
						}
					}
				}
			}

			// if we've reached this point and still havent found anything that wants to handle
			// the current request, we offer up a final chance to respond to this fact...
			if(OnPathNotFound != null)
			{
				OnPathNotFound((HttpApplication)sender, sprocketPath, pathSections, flag);
				if (flag.Handled)
				{
					pg.Response.End();
					return;
				}
			}

			// if we got this far, sorry folks, but you're about to get a boring ASP.Net 404 page.
		}

		/// <summary>
		/// Sprocket calls this method in response to ASP.Net's Error event, called when an unhandled
		/// exception is thrown.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void FireError(object sender, EventArgs e)
		{
			if(OnApplicationError != null)
			{
				HttpApplication app = (HttpApplication)sender;
				SystemEvents.Instance.NotifyExceptionThrown(app.Server.GetLastError());
				if(OnApplicationError != null)
					OnApplicationError(app, app.Server.GetLastError());
			}
		}

		/// <summary>
		/// This is our fancy little page for displaying any errors flagged during the SprocketSettings
		/// module's settings errors checking spree at the start.
		/// </summary>
		private void ShowErrorPage()
		{
			string html = ResourceLoader.LoadTextResource("Sprocket.Web.html.errorpage.htm");
			SprocketSettings.SettingsErrors errors = ((SprocketSettings)SystemCore.Instance["SprocketSettings"]).ErrorList;
			string str = "";
			foreach (KeyValuePair<string, List<string>> error in errors.List)
			{
				str += "<h2>Module: <span class=\"ModuleName\">" + SystemCore.Instance[error.Key].Title + "</span></h2><ul>";
				foreach (string msg in error.Value)
					str += "<li>" + msg + "</li>";
				str += "</ul>";
			}
			html = html.Replace("{body}", str);
			HttpContext.Current.Response.Write(html);
			HttpContext.Current.Response.End();
		}

		public static WebEvents Instance
		{
			get { return (WebEvents)SystemCore.Instance["WebEvents"]; }
		}

		#region ISprocketModule
		public void AttachEventHandlers(ModuleRegistry registry)
		{
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "WebEvents"; }
		}

		public string Title
		{
			get { return "HttpApplication Event Manager"; }
		}

		public string ShortDescription
		{
			get { return "Provides the interface by which Sprocket hooks into the ASP.Net pipeline and associated events."; }
		}
		#endregion
	}

	public class HandleFlag
	{
		private bool flag=false;
		public bool Handled
		{
			get { return flag; }
		}

		public void Set()
		{
			flag = true;
		}
	}
}
