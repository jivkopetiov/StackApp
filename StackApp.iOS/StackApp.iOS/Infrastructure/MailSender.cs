using MonoTouch.UIKit;
using MonoTouch.MessageUI;

namespace Stacklash.iOS
{
	public class MailSender
	{
		private UIViewController _c;

		public MailSender (UIViewController c)
		{
			_c = c;
		}

		public void Show (string to = null, string subject = null, string body = null)
		{
			if (!MFMailComposeViewController.CanSendMail)
			{
				Alert.ShowErrorBox("Email is not setup correctly on this device");
				return;
			}

			var mail = new MFMailComposeViewController ();
			mail.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			mail.Finished += delegate {
				mail.DismissViewController(true, null);
			};

			if (!string.IsNullOrEmpty(to))
				mail.SetToRecipients(new string[] { to });

			if (!string.IsNullOrEmpty (subject))
				mail.SetSubject (subject);

			if (!string.IsNullOrEmpty (body))
				mail.SetMessageBody (body, true);

			_c.PresentViewController(mail, true, null);
		}
	}
}
