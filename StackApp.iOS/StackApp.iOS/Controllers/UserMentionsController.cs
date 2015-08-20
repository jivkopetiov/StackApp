using System;
using Abilitics.iOS;
using Stacklash.Core;
using MonoTouch.UIKit;

namespace Stacklash.iOS
{
	public class UserMentionsController : StackBaseListController<Comment>
	{
        private User _user;

        public UserMentionsController(User user)
        {
            Title = "User Mentions";
            PagingSectionIndex = 1;
            _user = user;
        }

        protected override System.Threading.Tasks.Task<BaseResponse<Comment>> GetStackData()
        {
            return ServiceProxy.GetUserMentions(_user.user_id, _page);
        }

        protected override ContentRow GetRow(Comment comment)
        {
            return new CommentListRow(comment);
        }

        protected override void BeforeAddingRows()
        {
            AddRow(new UserListRow(_user));
            AddHeaderRow("Mentions");
        }
	}
}
