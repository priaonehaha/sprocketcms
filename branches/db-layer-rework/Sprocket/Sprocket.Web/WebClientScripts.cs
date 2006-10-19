using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Sprocket.SystemBase;
using Sprocket.Utility;

namespace Sprocket.Web
{
	/// <summary>
	/// Handles loading and rendering of JavaScript on the client, including initialisation
	/// of the AJAX subsystem and mapping of server-side ajax methods to client-side methods
	/// and objects of the same names. JavaScripts are compressed using Atif Aziz's
	/// JavaScriptCondenser class, which strips out comments, whitespace, etc. Data formatting
	/// for transfer between the client and server is provided by the open-source JSON format.
	/// </summary>
	public class WebClientScripts : ISprocketModule
	{
		/// <summary>
		/// Provides the facility to add extra javascript blocks to the current page.
		/// </summary>
		/// <param name="scripts">Dictionary mapping a unique key name to a block of javascript code</param>
		public delegate void LoadingJavaScriptsHandler(Dictionary<string, string> scripts);

		/// <summary>
		/// Provides the facility for other modules to make modifications to the loaded javascripts
		/// before they are rendered.
		/// </summary>
		/// <param name="scripts">A reference to the javascript collection</param>
		public delegate void BeforeRenderJavaScriptHandler(JavaScriptCollection scripts);

		/// <summary>
		/// Fires after all javascript is loaded and before it is rendered.
		/// </summary>
		public event BeforeRenderJavaScriptHandler OnBeforeRenderJavaScript;

		public void Initialise(ModuleRegistry registry)
		{
		}

		public static WebClientScripts Instance
		{
			get { return (WebClientScripts)Core.Instance["WebClientScripts"]; }
		}

		/// <summary>
		/// Builds a set of javascript methods which are mapped to corresponding ModuleBase-derived
		/// class methods which are marked with the AjaxMethod attribute.
		/// </summary>
		/// <param name="registry">A reference to the module registry</param>
		/// <returns>A block of javascript defining objects and methods that encapsulate ajax calls to
		/// matching server-side methods</returns>
		private string GetAjaxMethodsScript(ModuleRegistry registry)
		{
			Hashtable modules = new Hashtable();

			foreach (RegisteredModule module in registry)
			{
				if (module.Module.GetType().GetCustomAttributes(typeof(AjaxMethodHandlerAttribute), false).Length != 1)
					continue;
				MethodInfo[] infos = module.Module.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
				bool nameAdded = false;
				foreach (MethodInfo info in infos)
				{
					object[] attrs = info.GetCustomAttributes(typeof(AjaxMethodAttribute), false);
					if (attrs.Length == 1)
					{
						if (!nameAdded)
						{
							nameAdded = true;
							modules.Add(module.Module.RegistrationCode, new ArrayList());
						}
						((ArrayList)modules[module.Module.RegistrationCode]).Add(info.Name);
					}
				}
			}

			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			sb.Append("Ajax = {};");
			sb.Append(Environment.NewLine);

			foreach (DictionaryEntry de in modules)
			{
				sb.Append("Ajax.");
				sb.Append(de.Key.ToString());
				sb.Append(" = {");
				sb.Append(Environment.NewLine);
				ArrayList arr = (ArrayList)de.Value;
				for (int i = 0; i < arr.Count; i++)
				{
					sb.Append("\t");
					sb.Append(arr[i]);
					sb.Append(" : function() { SprocketAjax.Request('");
					sb.Append(de.Key.ToString());
					sb.Append(".");
					sb.Append(arr[i]);
					sb.Append("', arguments); }");
					if (i < arr.Count - 1) sb.Append(",");
					sb.Append(Environment.NewLine);
				}
				sb.Append("}");
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		private string standardScripts = null;

		/// <summary>
		/// Writes all registered javascripts into a string surrounded by html script tags,
		/// ready to be written to an html page.
		/// </summary>
		/// <returns>HTML script tags with containing javascript</returns>
		public string BuildScriptTags()
		{
			if (standardScripts == null)
			{
				standardScripts = string.Concat(
					ResourceLoader.LoadTextResource("Sprocket.Web.javascript.generic.js"),
					ResourceLoader.LoadTextResource("Sprocket.Web.javascript.browser-tools.js"),
					ResourceLoader.LoadTextResource("Sprocket.Web.javascript.json.js"),
					ResourceLoader.LoadTextResource("Sprocket.Web.javascript.ajax.js"),
					GetAjaxMethodsScript(Core.Instance.ModuleRegistry)
					);
			}

			Dictionary<string, string> scripts = new Dictionary<string, string>();
			JavaScriptCollection jsc = new JavaScriptCollection();
			jsc.SetKey("$APPLICATIONROOT$", WebUtility.BasePath);
			jsc.Add("standard", standardScripts);

			if (OnBeforeRenderJavaScript != null) OnBeforeRenderJavaScript(jsc);

			return jsc.CreateScriptTags();
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "WebClientScripts"; }
		}

		public string Title
		{
			get { return "Web Client Script Renderer"; }
		}

		public string ShortDescription
		{
			get { return "Handles javascript aggregation and rendering to the page."; }
		}
	}

	/// <summary>
	/// Handles collection, preparation and rendering of javascript blocks.
	/// </summary>
	public class JavaScriptCollection
	{
		private Dictionary<string, string> scripts = new Dictionary<string,string>();
		private Dictionary<string, object> keys = new Dictionary<string,object>();
		/// <summary>
		/// Adds a new javascript block to the collection.
		/// </summary>
		/// <param name="name">A unique (arbitrary) code to identify this block</param>
		/// <param name="script">The script block</param>
		public void Add(string name, string script)
		{
			if (scripts.ContainsKey(name))
				throw new SprocketException("JavaScriptCollection: Can't add script of name \"" + name + "\" because it already exists in the collection.");
			scripts.Add(name, script);
		}

		/// <summary>
		/// Use this method to replace javascript placeholders with actual values. For
		/// example, the script block could have a value {AuthKey} somewhere in the various
		/// javascript blocks. This would be put in the script blocks to be replaced by a
		/// real authentication key string when the script is rendered to the page. All
		/// script blocks in the collection will have the specified key/string replaced by
		/// the specified value before the scripts are rendered to the page.
		/// </summary>
		/// <param name="keyName">The name of the key/string to replace at run-time</param>
		/// <param name="keyValue">The value to replace the key with</param>
		public void SetKey(string keyName, string keyValue)
		{
			keys[keyName] = keyValue;
		}

		/// <summary>
		/// Only used for ASP.Net web forms with server form tags. Registers all contained
		/// javascripts in script tags within the server form.
		/// </summary>
		public void RegisterScripts()
		{
            Page page = (Page)HttpContext.Current.Handler;
			foreach (KeyValuePair<string, string> script in scripts)
			{
				string js = script.Value;
				foreach (KeyValuePair<string, object> key in keys)
					js = js.Replace(key.Key, key.Value.ToString());
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), script.Key, JavaScriptCondenser.Condense(js));
			}
		}

		/// <summary>
		/// Writes all registered scripts into a single string surrounded by html script tags
		/// </summary>
		/// <returns>HTML script tags with containing javascript</returns>
		public string CreateScriptTags()
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, string> script in scripts)
			{
				string js = script.Value;
				foreach (KeyValuePair<string, object> key in keys)
					js = js.Replace(key.Key, key.Value.ToString());
				sb.Append("<script language=\"JavaScript\">");
				sb.Append(Environment.NewLine);
				if (SprocketSettings.GetValue("CompressJavaScript").ToLower() == "true")
					js = JavaScriptCondenser.Condense(js);
				sb.Append(js);
				sb.Append(Environment.NewLine);
				sb.Append("</script>");
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}
	}

	/// <summary>
	/// Marks a class method for use by the AJAX subsystem. This is only used
	/// by classes that implement ISprocketModule and are marked by the
	/// ModuleRegistrationCode attribute. JavaScript will be generated to define
	/// a client-side object for the class that will contain matching methods for
	/// each server-side method marked with this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	public class AjaxMethodAttribute : Attribute
	{
		private bool requiresAuthentication = false;

		public bool RequiresAuthentication
		{
			get { return requiresAuthentication; }
			set { requiresAuthentication = value; }
		}
	}

	/// <summary>
	/// Any class utilising the AjaxMethod attribute must be marked with
	/// the AjaxMethodHandler attribute or it will be ignored when
	/// Ajax javascript code is generated.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public class AjaxMethodHandlerAttribute : Attribute
	{
	}
}
