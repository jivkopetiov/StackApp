using System;
using MonoTouch.Foundation;
using System.Globalization;
using Newtonsoft.Json;

namespace Abilitics.iOS
{
	public class NSUserDefaultsProxy {

		public void SetNumber(string key, int? val) {
			if (val.HasValue)
				NSUserDefaults.StandardUserDefaults.SetInt (val.Value, key);
			else 
				NSUserDefaults.StandardUserDefaults.RemoveObject (key);

			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public int? GetNumber(string key) {

			string val = NSUserDefaults.StandardUserDefaults.StringForKey(key);
			if (string.IsNullOrEmpty (val))
				return null;

			return int.Parse (val);
		}

		public void SetString(string key, string val) {

			if (string.IsNullOrEmpty (val))
				NSUserDefaults.StandardUserDefaults.RemoveObject (key);
			else 
				NSUserDefaults.StandardUserDefaults.SetString (val, key);

			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public string GetString(string key) {
			return NSUserDefaults.StandardUserDefaults.StringForKey(key);
		}

		public void SetBool(string key, bool val) {
			NSUserDefaults.StandardUserDefaults.SetBool (val, key);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public bool GetBool(string key) {
			return NSUserDefaults.StandardUserDefaults.BoolForKey (key);
		}

		private static readonly string _dateFormat = "dd-MM-yyyy HH:mm:ss";

		public void SetNullableDate(string key, DateTime? val) {
			string dateString;
			if (val.HasValue)
				dateString = val.Value.ToString (_dateFormat, CultureInfo.InvariantCulture);
			else 
				dateString = "";

			NSUserDefaults.StandardUserDefaults.SetString (dateString, key);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public DateTime? GetNullableDate(string key) {
			string dateString = NSUserDefaults.StandardUserDefaults.StringForKey(key);
			if (string.IsNullOrEmpty (dateString))
				return null;

			return DateTime.ParseExact(dateString, _dateFormat, CultureInfo.InvariantCulture);
		}

		public T GetJsonObject<T>(string key) where T: class {

			string json = NSUserDefaults.StandardUserDefaults.StringForKey (key);
			if (string.IsNullOrEmpty (json))
				return null;

			return JsonConvert.DeserializeObject<T> (json);
		}

		public void SetJsonObject<T>(T data, string key) where T: class {
			string json = JsonConvert.SerializeObject(data);
			NSUserDefaults.StandardUserDefaults.SetString (json, key);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}
	}
	
}
