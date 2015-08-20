using Abilitics.iOS;
using System;

namespace Stacklash.iOS
{
	public class AboutController : RichListController
	{
		public override void ViewDidLoad ()
		{
			try {
                this.InitLeftButton();
				Title = "About Stacklash";

                var user = Config.CurrentUser;
                if (user == null)
                    AddValue1FixedRow ("User", "Not Logged-in");
                else 
                    AddValue1FixedRow("User", user.NameDecoded);

				AddValue1FixedRow ("App Version", IOS.GetBundleVersion ());
				AddValue1FixedRow ("iOS Version", IOS.GetIOSVersion ());
				AddSubtitleFixedRow ("Device Name", IOS.GetDeviceName ());
				AddSubtitleFixedRow ("Device Type", IOS.GetDeviceVersion ().ToString ());
				AddSubtitleFixedRow ("Bundle Identifier", IOS.GetBundleIdentifier ()); 

                AddRow(new ButtonRow { 
                    Text = "Send Feedback",
                    Action = delegate {
                        new MailSender(this).Show(subject: "Stacklash Feedback", to: Config.FeedbackEmail);
                    }
                });

                AddRow(new ButtonRow {
                    Text = "Clear Image Cache",
                    Action = delegate {
                        SDWebImage.SDImageCache.SharedImageCache.ClearMemory();
                        SDWebImage.SDImageCache.SharedImageCache.ClearDisk();
                    }
                });

				base.ViewDidLoad ();
			}
			catch (Exception ex) {
				this.UnhandledError(ex);                    
			}
		}
	}
}
