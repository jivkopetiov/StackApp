using System;
using Abilitics.iOS;
using Stacklash.Core;

namespace Stacklash.iOS
{
	public class InboxController : StackBaseListController<InboxItem>
	{
        public override void ViewDidLoad()
        {
            Title = "Inbox";
            this.InitLeftButton();
            base.ViewDidLoad();
        }

        protected override System.Threading.Tasks.Task<BaseResponse<InboxItem>> GetStackData()
        {
            return ServiceProxy.GetInbox(_page);
        }

        protected override ContentRow GetRow(InboxItem item)
        {
            return new InboxRow(item);
        }
	}
}
