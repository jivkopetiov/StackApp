using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog.Utilities;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Abilitics.iOS
{
	/// <summary>
	///    This interface needs to be implemented to be notified when an image
	///    has been downloaded.   The notification will happen on the UI thread.
	///    Upon notification, the code should call RequestImage again, this time
	///    the image will be loaded from the on-disk cache or the in-memory cache.
	/// </summary>
	public interface IImageUpdated {
		void UpdatedImage (string uri);
	}
	
	/// <summary>
	///   Network image loader, with local file system cache and in-memory cache
	/// </summary>
	/// <remarks>
	///   By default, using the static public methods will use an in-memory cache
	///   for 50 images and 4 megs total.   The behavior of the static methods 
	///   can be modified by setting the public DefaultLoader property to a value
	///   that the user configured.
	/// 
	///   The instance methods can be used to create different imageloader with 
	///   different properties.
	///  
	///   Keep in mind that the phone does not have a lot of memory, and using
	///   the cache with the unlimited value (0) even with a number of items in
	///   the cache can consume memory very quickly.
	/// 
	///   Use the Purge method to release all the memory kept in the caches on
	///   low memory conditions, or when the application is sent to the background.
	/// </remarks>

	public class ImageDownloader
	{
        public static UIImage TryToGetImageFromCache(string imageUrl) {
            if (string.IsNullOrEmpty (imageUrl))
                return null;

            string imageFileName = md5 (imageUrl);
            string fullPath = Path.Combine (PicDir, imageFileName);
            if (!File.Exists (fullPath))
                return null;

            return UIImage.FromFile (fullPath);
        }   

		public static void DeleteAllImagesFromCache() {
			if (Directory.Exists (PicDir)) {
				foreach (var file in Directory.GetFiles (PicDir)) {
					File.Delete(file);
				}
			}

			if (DefaultLoader != null)
				DefaultLoader.PurgeCache ();
		}

        private readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");
		private const int MaxRequests = 6;
		public static string PicDir; 
		
		// Cache of recently used images
		private LRUCache<string,UIImage> cache;
		
		// A list of requests that have been issues, with a list of objects to notify.
		private static Dictionary<string, List<IImageUpdated>> pendingRequests;
		
		// A list of updates that have completed, we must notify the main thread about them.
		private static HashSet<string> queuedUpdates;
		
		// A queue used to avoid flooding the network stack with HTTP requests
		private static Stack<string> requestQueue;
		
		private static NSString nsDispatcher = new NSString ("x");
		
		private static MD5CryptoServiceProvider checksum = new MD5CryptoServiceProvider ();
		
		/// <summary>
		///    This contains the default loader which is configured to be 50 images
		///    up to 4 megs of memory.   Assigning to this property a new value will
		///    change the behavior.   This property is lazyly computed, the first time
		///    an image is requested.
		/// </summary>
		public static ImageDownloader DefaultLoader;

		static ImageDownloader ()
		{
			PicDir = Path.Combine (BaseDir, "Library/Caches/Pictures.MonoTouch.Dialog/");
			
			if (!Directory.Exists (PicDir))
				Directory.CreateDirectory (PicDir);
			
			pendingRequests = new Dictionary<string,List<IImageUpdated>> ();
			queuedUpdates = new HashSet<string>();
			requestQueue = new Stack<string> ();
		}
		
		/// <summary>
		///   Creates a new instance of the image loader
		/// </summary>
		/// <param name="cacheSize">
		/// The maximum number of entries in the LRU cache
		/// </param>
		/// <param name="memoryLimit">
		/// The maximum number of bytes to consume by the image loader cache.
		/// </param>
		public ImageDownloader (int cacheSize, int memoryLimit)
		{
			cache = new LRUCache<string, UIImage> (cacheSize, memoryLimit, sizer);
		}
		
		private static int sizer (UIImage img)
		{
			var cg = img.CGImage;
			return cg.BytesPerRow * cg.Height;
		}
		
		/// <summary>
		///    Purges the cache of this instance of the ImageLoader, releasing 
		///    all the memory used by the images in the caches.
		/// </summary>
		public void PurgeCache ()
		{
			lock (cache)
				cache.Purge ();
		}
		
		private static int hex (int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v-10;
		}

		private static string md5 (string input)
		{
			var bytes = checksum.ComputeHash (Encoding.UTF8.GetBytes (input));
			var ret = new char [32];
			for (int i = 0; i < 16; i++){
				ret [i*2] = (char)hex (bytes [i] >> 4);
				ret [i*2+1] = (char)hex (bytes [i] & 0xf);
			}
			return new string (ret);
		}
		
		/// <summary>
		///   Requests an image to be loaded using the default image loader
		/// </summary>
		/// <param name="uri">
		/// The URI for the image to load
		/// </param>
		/// <param name="notify">
		/// A class implementing the IImageUpdated interface that will be invoked when the image has been loaded
		/// </param>
		/// <returns>
		/// If the image has already been downloaded, or is in the cache, this will return the image as a UIImage.
		/// </returns>
		public static UIImage DefaultRequestImage (string uri, IImageUpdated notify, Action<HttpWebRequest> webRequestInit = null)
		{
			if (DefaultLoader == null)
				DefaultLoader = new ImageDownloader (50, 4*1024*1024);
			return DefaultLoader.RequestImage (uri, notify, webRequestInit);
		}
		
		/// <summary>
		///   Requests an image to be loaded from the network
		/// </summary>
		/// <param name="uri">
		/// The URI for the image to load
		/// </param>
		/// <param name="notify">
		/// A class implementing the IImageUpdated interface that will be invoked when the image has been loaded
		/// </param>
		/// <returns>
		/// If the image has already been downloaded, or is in the cache, this will return the image as a UIImage.
		/// </returns>
		private UIImage RequestImage (string uri, IImageUpdated notify, Action<HttpWebRequest> webRequestInit)
		{
			UIImage ret;
			
			lock (cache){
				ret = cache [uri];
				if (ret != null)
					return ret;
			}

			lock (requestQueue){
				if (pendingRequests.ContainsKey (uri)) {
					if (!pendingRequests [uri].Contains(notify))
						pendingRequests [uri].Add (notify);
					return null;
				}				
			}

			string picfile = PicDir + md5 (uri);
			if (File.Exists (picfile)){
				ret = UIImage.FromFile (picfile);
				if (ret != null){
					lock (cache)
						cache [uri] = ret;
					return ret;
				}
			} 

			QueueRequest (uri, notify, webRequestInit);
			return null;
		}
		
		private static void QueueRequest (string uri, IImageUpdated notify, Action<HttpWebRequest> webRequestInit)
		{
			if (notify == null)
				throw new ArgumentNullException ("notify");
			
			lock (requestQueue){
				if (pendingRequests.ContainsKey (uri)){
					//Util.Log ("pendingRequest: added new listener for {0}", id);
					pendingRequests [uri].Add (notify);
					return;
				}
				var slot = new List<IImageUpdated> (4);
				slot.Add (notify);
				pendingRequests [uri] = slot;
				
				if (picDownloaders >= MaxRequests)
					requestQueue.Push (uri);
				else {
					ThreadPool.QueueUserWorkItem (delegate { 
							try {
								StartPicDownload (uri, webRequestInit); 
							} catch (Exception e){
								Console.WriteLine (e);
							}
						});
				}
			}
		}

		public static bool Download (string uri, Action<HttpWebRequest> webRequestInit = null)
		{
			try {

				var target =  PicDir + md5 (uri);
				Console.WriteLine ("downloading " + uri);
				var req = (HttpWebRequest)WebRequest.Create (uri);
				req.AllowAutoRedirect = true;
				req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				if (webRequestInit != null)
					webRequestInit(req);

				using (var response = req.GetResponse ().GetResponseStream())
					using (var writeStream = new FileStream(target, FileMode.Create, FileAccess.Write))
						response.CopyTo (writeStream);

				return true;

			} catch (Exception e) {
				Console.WriteLine ("Problem with {0} {1}", uri, e);
				return false;
			}
		}
		
		private static long picDownloaders;
		
		private static void StartPicDownload (string uri, Action<HttpWebRequest> webRequestInit)
		{
			Interlocked.Increment (ref picDownloaders);
			try {
				_StartPicDownload (uri, webRequestInit);
			} catch (Exception e){
				Console.Error.WriteLine ("CRITICAL: should have never happened {0}", e);
			}
			Interlocked.Decrement (ref picDownloaders);
		}
		
		private static void _StartPicDownload (string uri, Action<HttpWebRequest> webRequestInit)
		{
			do {
				bool downloaded;
				
				//System.Threading.Thread.Sleep (5000);
				downloaded = Download (uri, webRequestInit);
				//if (!downloaded)
				//	Console.WriteLine ("Error fetching picture for {0} to {1}", uri, target);
				
				// Cluster all updates together
				bool doInvoke = false;
				
				lock (requestQueue){
					if (downloaded){
						queuedUpdates.Add (uri);
					
						// If this is the first queued update, must notify
						if (queuedUpdates.Count == 1)
							doInvoke = true;
					} else
						pendingRequests.Remove (uri);

					// Try to get more jobs.
					if (requestQueue.Count > 0){
						uri = requestQueue.Pop ();
						if (uri == null){
							Console.Error.WriteLine ("Dropping request {0} because url is null", uri);
							pendingRequests.Remove (uri);
							uri = null;
						}
					} else {
						//Util.Log ("Leaving because requestQueue.Count = {0} NOTE: {1}", requestQueue.Count, pendingRequests.Count);
						uri = null;
					}
				}	
				if (doInvoke)
					nsDispatcher.BeginInvokeOnMainThread (NotifyImageListeners);
				
			} while (uri != null);
		}
		
		// Runs on the main thread
		private static void NotifyImageListeners ()
		{
			lock (requestQueue){
				foreach (var quri in queuedUpdates){
					var list = pendingRequests [quri];
					pendingRequests.Remove (quri);
					foreach (var pr in list){
						try {
							pr.UpdatedImage (quri);
						} catch (Exception e){
							Console.WriteLine (e);
						}
					}
				}
				queuedUpdates.Clear ();
			}
		}
	}
}
