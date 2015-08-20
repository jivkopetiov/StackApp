using System;
using Abilitics.iOS;
using Stacklash.Core;

namespace Stacklash.iOS
{
	public class UsersController : StackBaseListController<ShallowUser>
	{
        public override void ViewDidLoad()
        {
            Title = "Users";
            this.InitLeftButton();
            base.ViewDidLoad();
        }

        protected override System.Threading.Tasks.Task<BaseResponse<ShallowUser>> GetStackData()
        {
            return ServiceProxy.GetUsers(UserSortBy.reputation, _page);
        }

        protected override ContentRow GetRow(ShallowUser user)
        {
            return new UserListRow(user);
        }
	}
}
