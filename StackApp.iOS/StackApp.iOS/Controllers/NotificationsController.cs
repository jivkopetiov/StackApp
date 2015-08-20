using System;
using MonoTouch.UIKit;
using Abilitics.iOS;
using Stacklash.Core;
using MonoTouch.Foundation;
using SDWebImage;

namespace Stacklash.iOS
{
	public class NotificationsController : StackBaseListController<Notification>
	{
        private UIImage placeholder = UIImage.FromFile("noimage.png");

        public override void ViewDidLoad()
        {
            this.InitLeftButton();
            Title = "Notifications";
            base.ViewDidLoad();
        }

        protected override System.Threading.Tasks.Task<BaseResponse<Notification>> GetStackData()
        {
            return ServiceProxy.GetNotifications(_page);
        }

        protected override ContentRow GetRow(Notification item)
        {
            return new TitleDynamicRow {
                Text = BclEx.RemoveHtmlTags(item.body),
                Details = BclEx.OffsetFromNow(item.creation_date),
                AfterGetCellInit = cell => {
                    cell._image.SetImage(new NSUrl(item.site.logo_url), placeholder);
                }
            };
        }
	}
}
