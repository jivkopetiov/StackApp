using System;
using MonoTouch.UIKit;
using Abilitics.iOS;
using Stacklash.Core;

namespace Stacklash.iOS
{
	public class ModeratorsController : StackBaseListController<ShallowUser>
	{
        public ModeratorsController()
        {
            Title = "Moderators";
        }

        protected override System.Threading.Tasks.Task<BaseResponse<ShallowUser>> GetStackData()
        {
            return ServiceProxy.GetModerators(UserSortBy.reputation, _page);
        }

        protected override ContentRow GetRow(ShallowUser user)
        {
            return new UserListRow(user);
        }
	}
}
