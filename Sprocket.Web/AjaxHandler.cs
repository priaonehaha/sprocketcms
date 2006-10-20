using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Sprocket;
using Sprocket.SystemBase;
using Sprocket.Utility;

namespace Sprocket.Web
{
	/// <summary>
	/// An ISprocketModule implementation that plugs into the WebEvents module to handle
	/// requests where the URL ends with .ajax (which signifies an Ajax request) from the
	/// browser via the browser's XmlHttpRequest object. Ajax requests utilise the classes
	/// AjaxMethodHandlerAttribute and AjaxMethodAttribute for determining which classes
	/// and methods can be used in Ajax requests.
	/// </summary>
	[ModuleDependency("WebEvents")]
	[ModuleDependency("WebAuthentication")]
	public class AjaxRequestHandler : ISprocketModule
	{
		public static AjaxRequestHandler Instance
		{
			get { return (AjaxRequestHandler)SystemCore.Instance["AjaxRequestHandler"]; }
		}

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnBeginHttpRequest += new WebEvents.HttpApplicationCancellableEventHandler(OnBeginHttpRequest);
		}

		void OnBeginHttpRequest(HttpApplication app, HandleFlag handled)
		{
			if (handled.Handled)
				return;

			if (app.Context.Request.Path.EndsWith(".ajax"))
			{
				handled.Set();
				ProcessRequest(HttpContext.Current);
			}
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string Title
		{
			get { return "Ajax Request Handler"; }
		}

		public string ShortDescription
		{
			get { return "Processes requests from the XmlHttpRequest javascript object (Ajax calls)."; }
		}

		public string RegistrationCode
		{
			get { return "AjaxRequestHandler"; }
		}

		#endregion

		private static Guid authKey = Guid.Empty;
		public static Guid AuthKey
		{
			get { return authKey; }
		}

		/// <summary>
		/// Called when an AjaxMethod that has a RequiresAuthentication value of true
		/// passes the authentication check. This event can be used to perform further
		/// attribute checks on the method to further authenticate the calling user,
		/// such as for role and permission requirement checking.
		/// </summary>
		public event InterruptableEventHandler<MethodInfo> OnAjaxRequestAuthenticationCheck;

		/// <summary>
		/// This method is called in response to any request where the URL ends with a .ajax
		/// extension, which Sprocket uses to designate an Ajax request from an XmlHttpRequest
		/// call in the browser. The posted data contains information that this method uses to
		/// find the correct ISprocketModule implementation. It then uses reflection to get
		/// the requested method from the module, converts the javascript arguments into native
		/// CLR types and passes them to the method, after which it takes the returned data and
		/// writes it to the output stream ready for the XmlHttpRequest object to complete its
		/// call. Note that data transport uses JSON encoding. See the JSON class in the
		/// Sprocket.System namespace for information.
		/// </summary>
		/// <param name="context">The current HttpContext object.</param>
		internal void ProcessRequest(HttpContext context)
		{
			try
			{
				// load the post data from the http stream
				byte[] posted = new byte[context.Request.InputStream.Length];
				context.Request.InputStream.Read(posted, 0, posted.Length);

				// interpret the stream as a JSON string and parse it into a dictionary
				IDictionary<string, object> data = (IDictionary<string, object>)JSON.Parse(System.Text.Encoding.ASCII.GetString(posted));
				
				// extract the module and method name
				string[] func = data["ModuleName"].ToString().Split('.');
				if(func.Length != 2)
					throw new AjaxException("Method name specified incorrectly. Expected format ModuleName.MethodName.\nThe following incorrect format was supplied: " + data["ModuleName"]);
				string moduleName = func[0];
				string methodName = func[1];

				// extract the authentication key
				if (data["AuthKey"].ToString() != WebAuthentication.AuthKeyPlaceholder)
					authKey = new Guid(data["AuthKey"].ToString());

				// extract the arguments
				List<object> parsedArguments = (List<object>)data["MethodArgs"];

				// find and verify the module/method that should handle this request
				ISprocketModule module = SystemCore.Instance[moduleName];
				if(module == null)
					throw new AjaxException("The specified module \"" + func[0] + "\" was not found.");
				if(Attribute.GetCustomAttribute(module.GetType(), typeof(AjaxMethodHandlerAttribute), false) == null)
					throw new SprocketException("The specified module is not marked with AjaxMethodHandlerAttribute. (" + data["ModuleName"] + ")");
				MethodInfo info = module.GetType().GetMethod(methodName);
				if(info == null)
					throw new AjaxException("Failed to find an instance of the specified method. (" + data["ModuleName"] + ")");
				Attribute ajaxMethodAttr = Attribute.GetCustomAttribute(info, typeof(AjaxMethodAttribute));
				if(ajaxMethodAttr == null)
					throw new AjaxException("Specified method is not marked with AjaxMethodAttribute. (" + data["ModuleName"] + ")");
				AjaxMethodAttribute attr = (AjaxMethodAttribute)ajaxMethodAttr;
				if (attr.RequiresAuthentication)
				{
					WebAuthentication auth = (WebAuthentication)SystemCore.Instance["WebAuthentication"];
					if (!auth.IsLoggedIn)
						throw new AjaxException("You're not currently logged in. Please refresh the page.");

					if (OnAjaxRequestAuthenticationCheck != null)
					{
						Result result = new Result();
						OnAjaxRequestAuthenticationCheck(info, result);
						if (!result.Succeeded)
							throw new AjaxException(result.Message);
					}
				}
				
				// get all of the parameters that the method requires
				ParameterInfo[] methodParamInfos = info.GetParameters();

				// funcinfo is a string representation of the method format and is used for displaying meaningful errors
				string funcinfo = data["ModuleName"] + "(";
				if(methodParamInfos.Length > 0)
					funcinfo += methodParamInfos[0].ParameterType.Name + " " + methodParamInfos[0].Name;
				for(int j=1; j<methodParamInfos.Length; j++)
					funcinfo += ", " + methodParamInfos[j].ParameterType.Name + " " + methodParamInfos[j].Name;
				funcinfo += ")";
				if(methodParamInfos.Length != parsedArguments.Count)
					throw new AjaxException("Method expects " + methodParamInfos.Length + " argument(s) but instead received " + (parsedArguments.Count) + ". Expected format is:\n" + funcinfo);

				// create the parameter array and convert each supplied value to its native type
				object[] prmValuesForMethod = new object[methodParamInfos.Length];
				for(int i=0; i<prmValuesForMethod.Length; i++)
				{
					Type t = methodParamInfos[i].ParameterType;
					try
					{
						if (parsedArguments[i] == null)
							prmValuesForMethod[i] = null;
						else if (t.Name == "DateTime")
							prmValuesForMethod[i] = DateTime.Parse(parsedArguments[i].ToString());
						else if (t.IsArray || t.IsSubclassOf(typeof(IList)))
						{
							int elementCount = ((List<object>)parsedArguments[i]).Count;
							Type arrType = t.GetElementType().MakeArrayType(elementCount);
							object arr = Array.CreateInstance(t.GetElementType(), ((List<object>)parsedArguments[i]).Count);
							for (int k = 0; k < ((IList<object>)parsedArguments[i]).Count; k++)
								((IList)arr)[k] = ((IList<object>)parsedArguments[i])[k];
							prmValuesForMethod[i] = arr;
						}
						else if (t.GetInterface("IJSONReader") != null)
						{
							object obj = Activator.CreateInstance(t);
							((IJSONReader)obj).LoadJSON(parsedArguments[i]);
							prmValuesForMethod[i] = obj;
						}
						else if (t.IsAssignableFrom(typeof(Guid)) || t.IsAssignableFrom(typeof(Guid?)))
							prmValuesForMethod[i] = parsedArguments[i] == null ? (Guid?)null : new Guid(parsedArguments[i].ToString());
						else if (t.Name.StartsWith("Nullable`"))
							prmValuesForMethod[i] = Convert.ChangeType(parsedArguments[i], t.GetGenericArguments()[0]);
						else
							prmValuesForMethod[i] = Convert.ChangeType(parsedArguments[i], t);
					}
					catch(Exception ex)
					{
						string err = "Error converting parameter {0} to type {1}. Expected format is:\n{2}\nReceived Value:\n{3}\nException message:\n{4}";
						throw new AjaxException(string.Format(err, i, methodParamInfos[i].ParameterType.Name, funcinfo, parsedArguments[i], ex));
					}
				}
				
				// invoke the method

				if(info.ReturnType == typeof(void))
				{
					info.Invoke(module, prmValuesForMethod);
					context.Response.Write("{}");
				}
				else
				{
					object returnVal = info.Invoke(module, prmValuesForMethod);
					context.Response.Write(JSON.Encode(returnVal));
				}
			}
			catch(Exception e)
			{
				StringWriter writer = new StringWriter();
				JSON.EncodeCustomObject(writer, new KeyValuePair<string, object>("__error", e));
				context.Response.Write(writer.ToString());
			}
		}
	}
}
