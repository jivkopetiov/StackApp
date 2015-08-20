using System;
using MonoTouch.UIKit;
using Stacklash.Core;
using MonoTouch.Foundation;

namespace Stacklash.iOS
{
	public class UserBioController : UIViewController 
	{
        private User _user;
        private UIWebView webView;

        public UserBioController(User user)
        {
            Title = "About";
            _user = user;
        }

        public override void ViewWillAppear(bool animated)
        {
            webView.Frame = View.Frame;
        }

        public override void ViewDidLoad()
        {
            try {
                webView = new UIWebView();
                webView.ScrollView.Bounces = false;
                Add(webView);

                webView.ShouldStartLoad += delegate(UIWebView v, NSUrlRequest request, UIWebViewNavigationType navigationType) {
                    if (navigationType == UIWebViewNavigationType.LinkClicked)
                    {
                        var c = new WebBrowserController(request.Url.AbsoluteString);
                        NavigationController.PushViewController(c, true);
                        return false;
                    }

                    return true;
                };

                webView.LoadHtmlString(_user.about_me, new NSUrl("http://" + Config.CurrentSite.api_site_parameter));
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }
	}
}
