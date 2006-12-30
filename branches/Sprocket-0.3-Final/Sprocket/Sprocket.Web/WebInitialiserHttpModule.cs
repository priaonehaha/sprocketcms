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
			int c = 0;
			switch (InitState)
			{
				case InitialisationState.InProgress:
					while (InitState == InitialisationState.InProgress)
					{
						System.Threading.Thread.Sleep(500);
						if (++c > 30)
						{
							HttpContext.Current.Response.Write("The website is being initialised. Please try again in a minute or so.");
							HttpContext.Current.Response.End();
						}
					}
					break;

				case InitialisationState.None:
					InitState = InitialisationState.InProgress;
					if (((HttpApplication)sender).Application["Sprocket_SystemCore_Instance"] == null)
						InitialiseSystemCore((HttpApplication)sender);
					InitState = InitialisationState.Complete;
					break;

			}
			((WebEvents)SystemCore.Instance["WebEvents"]).FireBeginRequest(sender, e);
		}

		enum InitialisationState
		{
			None,
			InProgress,
			Complete
		}

		InitialisationState InitState
		{
			get
			{
				InitialisationState state;
				if (HttpContext.Current.Application["Sprocket_InitialisationState"] == null)
					state = InitialisationState.None;
				else
					state = (InitialisationState)HttpContext.Current.Application["Sprocket_InitialisationState"];
				return state;
			}
			set
			{
				HttpContext.Current.Application["Sprocket_InitialisationState"] = value;
			}
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
