using System;
using Abilitics.iOS;
using Stacklash.Core;
using System.Linq;

namespace Stacklash.iOS
{
	public class AnswerDetailsController : StackBaseListController<Comment>
	{
        private int _postId;
        private Post _answer;

        public AnswerDetailsController (Post post)
        {
            _answer = post;
        }

        public AnswerDetailsController (int postId)
        {
            _postId = postId;
        }

        public async override void ViewDidLoad()
        {
            Title = "Answer";
            PagingSectionIndex = 1;

            base.ViewDidLoad();

            if (_answer == null || _answer.BodyDecoded.IsNullOrEmpty()) {
                var response = await ServiceProxy.GetAnswerById(_postId);
                _answer = response.items.First();
            }
        }

        protected override System.Threading.Tasks.Task<BaseResponse<Comment>> GetStackData()
        {
            return ServiceProxy.GetAnswerComments(_answer.answer_id, PostSortBy.votes, _page);
        }

        protected override ContentRow GetRow(Comment comment)
        {
            return new CommentListRow(comment);
        }

        protected override void BeforeAddingRows()
        {
            AddRow(new AnswerListRow(_answer, isDetails: true));

            AddRow(new WebViewWithCodeRow { Html = _answer.body });

            AddHeaderRow("Comments");
        }
	}
}
