using System;
using Abilitics.iOS;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace Stacklash.iOS
{
    public class WebViewWithCodeRow : CustomContentRow
    {
        public string Html;
        private InnerCell cell;

        private class InnerCell : UITableViewCell {

            private UIWebView _webView;
            private bool _loaded;
            public float height = 100;
            private UITableView _tableView;

            public InnerCell (string reuseIdentifier, UITableView tableView) : base(UITableViewCellStyle.Default, reuseIdentifier)
            {
                _tableView = tableView;
                _webView = new UIWebView();
                _webView.ScrollView.Bounces = false;

                _webView.LoadFinished += delegate {
                    Console.WriteLine ("load finished");
                    string jsResult = _webView.EvaluateJavascript("document.body.scrollHeight;");

                    height = int.Parse(jsResult);

                    _tableView.ReloadData();
                };

                _webView.LoadError += delegate {
                    Console.WriteLine ("load error");
                };

                Add(_webView);
            }

            public void ReloadUI(WebViewWithCodeRow row, UITableView tableView) {

                if (!_loaded)
                {
                    if (row.Html != null)
                        _webView.LoadHtmlString(GetPrettifyHtml(row.Html), NSBundle.MainBundle.ResourceUrl);

                    _loaded = true;
                }

                _webView.Frame = new RectangleF(0, 0, tableView.Frame.Width, height);
            }

            private static string GetPrettifyHtml(string html) {
                string bodyNormalized = html.Replace("<pre>", "<pre class='prettyprint'>");

                return string.Format(
                    @"<html><head>
                            <link rel='stylesheet' type='text/css' href='google-code-prettify/prettify.css' />
                            <script type='text/javascript' src='google-code-prettify/run_prettify.js'></script>
                          </head><body>{0}</body></html>", 
                    bodyNormalized);

            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            if (cell == null) {
                cell = new InnerCell("webviewcell", tableView);
            }

            cell.ReloadUI(this, tableView);

            return cell;
        }

        public override float GetHeight(UITableView tableView)
        {
            if (cell == null)
                return 100;
            else 
                return cell.height;
        }
    }
}
