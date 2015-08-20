using System;
using Stacklash.Core;
using System.Threading.Tasks;
using Abilitics.iOS;

namespace Stacklash.iOS
{
	public class AnswersForUserController : StackBaseListController<Post>
	{
        private int _userId;

        public AnswersForUserController (int userId)
        {
            _userId = userId;
            Title = "Answers";
        }

        protected override Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetAnswersForUser(_userId, PostSortBy.votes, _page);
        }

        protected override ContentRow GetRow(Post item)
        {
            return new AnswerListRow(item, isDetails: false)
            {
                NavController = () => new AnswerDetailsController(item)
            };
        }
	}
}
