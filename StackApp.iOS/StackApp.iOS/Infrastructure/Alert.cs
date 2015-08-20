using System;
using MonoTouch.UIKit;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace Stacklash.iOS
{
	public static class Alert
	{
		public static void ShowOkBox(string title, string message) {
			var alertView = new UIAlertView ();
			alertView.Title = title;
			alertView.Message = message;
			alertView.AddButton ("OK");
			alertView.Show ();
		}

        public async static void UnhandledError(this UIViewController c, Exception ex) {

            Console.WriteLine("Unhandled error:" + ex);

            int buttonIndex = await Message("Error", "Oops, the chaos monkey strikes again. Please send us the error message.", "Send Error", "Dismiss");
        
            if (buttonIndex == 0)
            {
                string body = ex.ToString();
                new MailSender(c).Show(subject: "Stacklash error", to: Config.FeedbackEmail, body: body);
            }
        }

		public static void ShowErrorBox (Exception ex)
		{
			ShowErrorBox(ex.ToString());
		}

		public static void ShowErrorBox (string message)
		{
			Console.WriteLine ("Showing error box on screen: " + message);
			var alertView = new UIAlertView ();
			alertView.Title = "Error";
			alertView.Message = message;
			alertView.AddButton ("OK");
			alertView.Show ();
		}

		private static async Task<int> Message (string title, string message, params string[] buttons) {

			var a = new UIAlertView(title ?? "", message ?? "", null, buttons.First (), buttons.Skip (1).ToArray ());

			var clicked = a.GetClickedEventAsync<UIButtonEventArgs>();
			a.Show ();
			return (await clicked).ButtonIndex;
		}

		private static Task<T> GetClickedEventAsync<T> (this UIAlertView eventSource) where T : UIButtonEventArgs {
			var tcs = new TaskCompletionSource<T>();

			EventHandler<UIButtonEventArgs> handler;

			handler = (sender, args) => {
				eventSource.Clicked -= handler;
				tcs.SetResult ((T)args);
			};

			eventSource.Clicked += handler;
			return tcs.Task;
		}
	}
}
