using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Stacklash.Core;
using System.Threading.Tasks;
using System.Linq;

namespace Stacklash.iOS
{
    public class LoginController : UIViewController
    {
        private UIWebView _webView;

        public override void ViewDidLoad()
        {
            Title = "Login";
            _webView = new UIWebView();
            _webView.ScalesPageToFit = true;
            _webView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            Add(_webView);

            _webView.LoadRequest(new NSUrlRequest(new NSUrl(ServiceProxy.GetLoginUrl())));

            _webView.ShouldStartLoad = delegate(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType) {

                if (request.Url.AbsoluteString.StartsWithOrdinalNoCase("https://stackexchange.com/oauth/login_success#")) {
                    string hash = request.Url.AbsoluteUrl.Fragment;
                    string accessToken = hash.Trim('#').Replace("access_token=", "");
                    Config.AccessToken = accessToken;

                    Task.Run(async delegate {
                        try {
                            Console.WriteLine ("starting request for get me");
                            var response = await ServiceProxy.GetMe();
                            Console.WriteLine ("got me");
                            Config.CurrentUser = response.items.First();

                            InvokeOnMainThread(delegate{
                                Nav.BuildDeck();
                            });
                        }
                        catch (Exception ex) {
                            InvokeOnMainThread(delegate {
                                this.UnhandledError(ex);
                                Config.AccessToken = null;
                            });
                        }
                    });

                    return false;
                }

                return true;
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            _webView.Frame = View.Frame;
        }
    }
}
