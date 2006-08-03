using System;
using System.Web;
using Sprocket.SystemBase;
using System.Diagnostics;

namespace Sprocket.Web
{
	/// <summary>
	/// This class implements the IHttpModule that is required to connect the Sprocket framework
	/// to the ASP.Net pipeline. It is interdependent with the WebEvents class.
	/// </summary>
	public class WebInitialiserHttpModule : IHttpModule
	{
		/// <summary>
		/// This is called just once when the HttpApplication starts up.
		/// </summary>
		/// <param name="app">The HttpApplication object that is to persist between requests.</param>
		public void Init(HttpApplication app)
		{
			app.BeginRequest += new EventHandler(app_BeginRequest);
			app.AcquireRequestState += new EventHandler(app_AcquireRequestState);
			app.EndRequest += new EventHandler(app_EndRequest);
			app.Error += new EventHandler(app_Error);
		}

		void app_Error(object sender, EventArgs e)
		{
			((WebEvents)SystemCore.Instance["WebEvents"]).FireError(sender, e);
		}

		void app_EndRequest(object sender, EventArgs e)
		{
			((WebEvents)SystemCore.Instance["WebEvents"]).FireEndRequest(sender, e);
		}

		void app_AcquireRequestState(object sender, EventArgs e)
		{
			((WebEvents)SystemCore.Instance["WebEvents"]).FireAcquireRequestState(sender, e);
		}

		void app_BeginRequest(object sender, EventArgs e)
		{
			if (((HttpApplication)sender).Application["Sprocket_SystemCore_Instance"] == null)
				InitialiseSystemCore((HttpApplication)sender);
			((WebEvents)SystemCore.Instance["WebEvents"]).FireBeginRequest(sender, e);
		}

		/// <summary>
		/// There's some nasty overhead here and this can take several seconds to run. Luckily it
		/// only runs on the very first request when the HttpApplication is starting up for the
		/// first time. This is not called again until a new HttpApplication object needs to be
		/// created, at which point the process is repeated.
		/// </summary>
		/// <param name="app"></param>
		void InitialiseSystemCore(HttpApplication app)
		{
			SystemCore core = new SystemCore();
			SystemCore.Instance = core;
			core.Initialise();
		}

		public void Dispose()
		{
		}
	}
}
