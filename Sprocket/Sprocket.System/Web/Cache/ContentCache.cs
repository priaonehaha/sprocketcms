using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Security.Cryptography;

using Sprocket;
using Sprocket.Web;

namespace Sprocket.Web.Cache
{
	/// <summary>
	/// The ContentCache class provides a method where generated content can be cached to disk
	/// for future requests to speed up further calls to specific URLs. The ASP.Net framework
	/// provides a caching mechanism also, but it caches directly to memory, which is very fast,
	/// but could possibly create a problem if we're trying to cache hundreds or thousands of
	/// pages for an unknown period of time.
	/// </summary>
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDescription("Simple replacement for the ASP.Net cache controls. Handles caching of requested content to disk instead of memory")]
	[ModuleTitle("Content Cache")]
	public class ContentCache : ISprocketModule
	{
		// handle caching to memory
		// check if a file is cached
		// cache a file somewhere
		// check if the cache is too big and reduce the size if it is (TrimToSize, TrimFromSize / Mb)
		
		// DiskCacheMaxSize 5gb
		// DiskCacheTrimSize 1gb

		private long diskCacheMaxSize = -1, diskCacheTrimSize = -1;
		private long DiskCacheMaxSize
		{
			get
			{
				if (diskCacheMaxSize == -1)
					if (!long.TryParse(SprocketSettings.GetValue("DiskCacheMaxSize"), out diskCacheMaxSize))
						diskCacheMaxSize = 500000000;
				return diskCacheMaxSize;
			}
		}
		private long DiskCacheTrimSize
		{
			get
			{
				if (diskCacheTrimSize == -1)
					if (!long.TryParse(SprocketSettings.GetValue("DiskCacheTrimSize"), out diskCacheTrimSize))
						diskCacheTrimSize = 50000000;
				return diskCacheTrimSize;
			}
		}

		public static ContentCache Instance
		{
			get { return (ContentCache)Core.Instance[typeof(ContentCache)].Module; }
		}

		public static Stream Retrieve(string identifier)
		{
			Stream stream;
			if (TryRetrieve(identifier, out stream))
				return stream;
			return null;
		}

		public static string RetrieveText(string identifier)
		{
			string text;
			if (TryRetrieveText(identifier, out text))
				return text;
			return null;
		}

		public static bool TryRetrieve(string identifier, out Stream stream)
		{
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			stream = cache.Read(identifier);
			return stream != null;
		}

		public static bool TryRetrieveText(string identifier, out string text)
		{
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			text = cache.ReadText(identifier);
			return text != null;
		}

		public static void Store(string identifier, TimeSpan? expiresAfter, bool forceExpiryAfterDuration, Stream data)
		{
			CacheItemInfo info = new CacheItemInfo(identifier, expiresAfter, forceExpiryAfterDuration, null, null);
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			if (cache.DiskSpaceUsed >= Instance.DiskCacheMaxSize)
				cache.FlushCache(Instance.DiskCacheTrimSize);
			cache.Write(info, data);
		}

		public static string Store(string identifier, TimeSpan? expiresAfter, bool forceExpiryAfterDuration, string sprocketPath, string contentType, Stream data)
		{
			CacheItemInfo info = new CacheItemInfo(identifier, expiresAfter, forceExpiryAfterDuration, sprocketPath, contentType);
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			if (cache.DiskSpaceUsed >= Instance.DiskCacheMaxSize)
				cache.FlushCache(Instance.DiskCacheTrimSize);
			cache.Write(info, data);
			return info.PhysicalPath;
		}

		public static string StoreText(string identifier, TimeSpan? expiresAfter, bool forceExpiryAfterDuration, string text)
		{
			CacheItemInfo info = new CacheItemInfo(identifier, expiresAfter, forceExpiryAfterDuration, null, null);
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			if (cache.DiskSpaceUsed >= Instance.DiskCacheMaxSize)
				cache.FlushCache(Instance.DiskCacheTrimSize);
			cache.WriteText(info, text);
			return info.PhysicalPath;
		}

		public static void StoreText(string identifier, TimeSpan? expiresAfter, bool forceExpiryAfterDuration, string sprocketPath, string contentType, string text)
		{
			CacheItemInfo info = new CacheItemInfo(identifier, expiresAfter, forceExpiryAfterDuration, sprocketPath, contentType);
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			if (cache.DiskSpaceUsed >= Instance.DiskCacheMaxSize)
				cache.FlushCache(Instance.DiskCacheTrimSize);
			cache.WriteText(info, text);
		}

		public static void Clear(string identifier)
		{
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			cache.Clear(identifier);
		}

		/// <param name="identifierPartialMatch">SQL Syntax for a LIKE clause. e.g. to clear all cached items with identifiers
		/// starting with "Item", specify "Item%".</param>
		public static void ClearMultiple(string identifierPartialMatch)
		{
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			cache.ClearPartialMatches(identifierPartialMatch);
		}

		public static bool Transmit(string sprocketPath)
		{
			CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
			return cache.Transmit(HttpContext.Current.Response, sprocketPath);
		}

		void SprocketSettings_OnSettingsVerified()
		{
			try
			{
				HttpContext.Current.Application.Lock();
				CacheManager cache = HttpContext.Current.Application[appStateKey] as CacheManager;
				if (cache == null)
				{
					cache = new CacheManager();
					cache.Initialise();
					HttpContext.Current.Application[appStateKey] = cache;
				}
			}
			finally
			{
				HttpContext.Current.Application.UnLock();
			}
		}

		void ContentCache_OnLoadRequestedPath(HandleFlag handled)
		{
			if (File.Exists(SprocketPath.Physical))
				return; // the cache never deals with paths that directly map to actual physical files
			DateTime dt = DateTime.Now;
			if (Transmit(SprocketPath.Value))
			{
				HttpContext.Current.Response.End();
				handled.Set();
			}
			TimeSpan ts = DateTime.Now - dt;
			LogFile.Append("writetimes.txt", ts.ToString() + " - " + SprocketPath.Value);
		}

		private const string appStateKey = "Sprocket.Web.Cache.CacheManager";

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			SprocketSettings.Instance.OnSettingsVerified += new EmptyHandler(SprocketSettings_OnSettingsVerified);
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(ContentCache_OnLoadRequestedPath);
		}
	}
}
