using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace Stacklash.iOS
{
	public class WebBrowserController : UIViewController
	{
		private string _url;
		
		private UIWebView _webView;
		
		private UIToolbar _bottomToolbar;
		
		private UIBarButtonItem _navigateBackButton;
		private UIBarButtonItem _navigateForwardButton;
		private UIBarButtonItem _refreshButton;
		
		private UIBarButtonItem _rightPlaceholderButton;
		
		public WebBrowserController (string url)
		{
			Title = url;
			_url = url;
		}
		
		protected void UpdateTitle ()
		{
			string title = _webView.EvaluateJavascript ("document.title");
			if (!string.IsNullOrEmpty (title))
				Title = title;
			else 
				Title = _webView.Request.Url.AbsoluteString;
		}
		
		private void UpdateNavButtons ()
		{
			if (_webView == null)
				return;

			_navigateBackButton.Enabled = _webView.CanGoBack;
			_navigateForwardButton.Enabled = _webView.CanGoForward;
		}
		
		private class WebDelegate : UIWebViewDelegate {
			
		}
		
		public override void ViewDidLoad ()
		{
			var placeholderView = new UIView(new RectangleF(0, 0, 34, 44));
			_rightPlaceholderButton = new UIBarButtonItem(placeholderView); 
			NavigationItem.RightBarButtonItem = _rightPlaceholderButton;
			
			base.ViewDidLoad();
			
			View.UserInteractionEnabled = true;
			View.MultipleTouchEnabled = true;
			
			_webView = new UIWebView();
			_webView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_webView.UserInteractionEnabled = true;
			_webView.MultipleTouchEnabled = true;
			_webView.Delegate = new WebDelegate();
			_webView.ScalesPageToFit = true;
			
			_webView.LoadStarted += delegate {
				_refreshButton.Enabled = false;
				UpdateNavButtons ();
			};
			
			_webView.LoadFinished += delegate {
				_refreshButton.Enabled = true;
				UpdateNavButtons ();
				UpdateTitle ();
			};
			
			_webView.LoadError += (webView, error) => {
				Console.WriteLine ("Failed " + error.Error.ToString ());
				BTProgressHUD.ShowErrorWithStatus ("Failed to load page");

				NSTimer.CreateScheduledTimer(2d, delegate {
					BTProgressHUD.Dismiss ();
				});
			};
			
			View.AddSubview(_webView);
			
			_bottomToolbar = new UIToolbar();
			_bottomToolbar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			View.AddSubview(_bottomToolbar);
			
			_navigateBackButton = new UIBarButtonItem(UIImage.FromFile("back.png"), UIBarButtonItemStyle.Plain, delegate {
				_webView.GoBack();
			});
			
			_navigateForwardButton = new UIBarButtonItem(UIImage.FromFile("forward.png"), UIBarButtonItemStyle.Plain, delegate {
				_webView.GoForward();
			});
			
			_refreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh, delegate {
				_webView.Reload();
			});
			
			_bottomToolbar.SetItems(new [] { 
				_navigateBackButton, 
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 5 },
				_navigateForwardButton, 
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
				_refreshButton }, 
			false);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear(animated);
			LayoutSubviews();
			
			if (!IOS.IsGoogleReachable())
			{
				BTProgressHUD.ShowErrorWithStatus ("No Internet Connection");

				NSTimer.CreateScheduledTimer(2d, delegate {
					BTProgressHUD.Dismiss ();
				});
			}
			else {
				_webView.LoadRequest(new NSUrlRequest(new NSUrl(_url)));
			}
		}
		
		private void LayoutSubviews() {
			_webView.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height - 44);
			_bottomToolbar.Frame = new RectangleF(0, View.Frame.Height - 44, View.Frame.Width, 44);
		}
	}
}
