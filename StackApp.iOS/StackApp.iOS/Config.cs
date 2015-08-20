using Abilitics.iOS;
using Stacklash.Core;
using Newtonsoft.Json;

namespace Stacklash.iOS
{
	public class Config
	{
		private static readonly NSUserDefaultsProxy proxy = new NSUserDefaultsProxy ();
        public const string FeedbackEmail = "jivkopetiov@gmail.com";

        public const bool IsDebug = true;

		public static Site CurrentSite {
            get { 
                string siteStr = proxy.GetString ("CurrentSite");
                if (string.IsNullOrEmpty(siteStr))
                    return null;

                return JsonConvert.DeserializeObject<Site>(siteStr);
            }
            set { 
                proxy.SetString ("CurrentSite", JsonConvert.SerializeObject(value)); 
            } 
		}

        public static User CurrentUser
        {
            get
            { 
                string siteStr = proxy.GetString("CurrentUser");
                if (string.IsNullOrEmpty(siteStr))
                    return null;

                return JsonConvert.DeserializeObject<User>(siteStr);
            }
            set { 
                proxy.SetString("CurrentUser", JsonConvert.SerializeObject(value)); 
            } 
        }

        public static string AccessToken {
            get { return proxy.GetString("AccessToken"); }
            set { proxy.SetString("AccessToken", value); }
        }
	}
}
