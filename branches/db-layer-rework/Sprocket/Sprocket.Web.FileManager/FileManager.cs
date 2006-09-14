using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Drawing;

using Sprocket;
using Sprocket.SystemBase;
using Sprocket.Web;
using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Web.FileManager
{
	[ModuleDependency("WebEvents")]
	[ModuleDependency("DatabaseManager")]
	[ModuleDependency("SecurityProvider")]
	public class FileManager : ISprocketModule, IDataHandlerModule
	{
		public event InterruptableEventHandler<SprocketFile> OnBeforeSprocketFileServed;
		public event NotificationEventHandler<SprocketFile> OnSprocketFileServed;
		public event NotificationEventHandler<SprocketFile> OnSprocketFileDeleted;

		#region Event Handlers

		void OnLoadRequestedPath(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (handled.Handled) return;
			if (sprocketPath.StartsWith("datastore/filemanager/"))
			{	// deny access if the directory is accessed directly
				handled.Set();
				return;
			}
			SprocketFile file = LoadCacheSprocketFile(sprocketPath);
			if (file == null) return;
			if (!File.Exists(file.PhysicalPath))
				throw new SprocketException("A file has been requested that is handled by the FileManager. "
					+ "The file has a record in the database but the accompanying file is missing. The ID for "
					+ "the file is " + file.SprocketFileID + " and the Sprocket path is " + file.SprocketPath + ".");
			handled.Set();
			if (OnBeforeSprocketFileServed != null)
			{
				Result result = new Result();
				OnBeforeSprocketFileServed(file, result); // allow other modules to deny access to the file
				if (!result.Succeeded) return;
			}
			if (OnSprocketFileServed != null)
				OnSprocketFileServed(file);
			HttpContext.Current.Response.TransmitFile(file.PhysicalPath);
			HttpContext.Current.Response.ContentType = file.ContentType;
		}

		#endregion

		#region Private Methods

		private void RemoveSprocketFileFromCache(string sprocketPath)
		{
			Dictionary<string, SprocketFile> files = (Dictionary<string, SprocketFile>)HttpContext.Current.Application["SprocketFileManager"];
			if(files[sprocketPath] == null) return;
			files.Remove(sprocketPath);
		}

		private SprocketFile LoadCacheSprocketFile(string sprocketPath)
		{
			if (HttpContext.Current.Application["SprocketFileManager"] == null)
				HttpContext.Current.Application["SprocketFileManager"] = new Dictionary<string, SprocketFile>();
			Dictionary<string, SprocketFile> files = (Dictionary<string, SprocketFile>)HttpContext.Current.Application["SprocketFileManager"];

			SprocketFile file;
			if (!files.ContainsKey(sprocketPath))
			{
				file = SprocketFile.Load(sprocketPath);
				files[sprocketPath] = file;
			}
			else
				file = files[sprocketPath];
			return file;
		}

		#endregion

		public static FileManager Instance
		{
			get { return (FileManager)SystemCore.Instance["FileManager"]; }
		}

		public SprocketFile SpawnThumbnail(SprocketFile imageFile)
		{
			if (!Utility.Utilities.MatchesAny(imageFile.FileTypeExtension.ToLower(), "gif", "jpg", "png"))
				throw new SprocketException("The specified file is not an image. No thumbnail can be created.");
			return null;
		}

		internal void NotifyFileDeleted(SprocketFile file)
		{
			object obj = HttpContext.Current.Application["SprocketFileManager"];
			if (obj == null) return;
			Dictionary<string, SprocketFile> cache = (Dictionary<string, SprocketFile>)obj;
			if(cache.ContainsKey(file.SprocketPath))
				cache.Remove(file.SprocketPath);

			if (OnSprocketFileDeleted != null)
				OnSprocketFileDeleted(file);
		}

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents w = (WebEvents)SystemCore.Instance["WebEvents"];
			w.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
		}

		public void Initialise(ModuleRegistry registry)
		{
			Directory.CreateDirectory(WebUtility.MapPath("datastore/filemanager/uploads"));
			Directory.CreateDirectory(WebUtility.MapPath("datastore/filemanager/thumbnails"));
		}

		public string RegistrationCode
		{
			get { return "FileManager"; }
		}

		public string Title
		{
			get { return "Sprocket File Manager"; }
		}

		public string ShortDescription
		{
			get { return "Handles storage and transmission of physical files to the client"; }
		}

		#endregion

		#region IDataHandlerModule Members

		public void ExecuteDataScripts()
		{
			((SqlDatabase)Database.Main).ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Web.FileManager.DatabaseScripts.tables.sql"));
			((SqlDatabase)Database.Main).ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Web.FileManager.DatabaseScripts.procedures.sql"));
		}

		#endregion
	}
}
