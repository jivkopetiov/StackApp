using System;
using Abilitics.iOS;
using Stacklash.Core;
using System.Linq;

namespace Stacklash.iOS
{
	public class QuestionBodyController : StackBaseListController<Comment>
	{
        private Post _post;

        public QuestionBodyController(Post post)
        {
            _post = post;
        }

        public async override void ViewDidLoad()
        {
            Title = "Question Details";
            PagingSectionIndex = 1;

            base.ViewDidLoad();

            if (_post.BodyDecoded.IsNullOrEmpty()) {
                var response = await ServiceProxy.GetPostById(_post.question_id);
                _post = response.items.First();
            }
        }

        protected override System.Threading.Tasks.Task<BaseResponse<Comment>> GetStackData()
        {
            return ServiceProxy.GetQuestionComments(_post.question_id, PostSortBy.creation, _page);
        }

        protected override ContentRow GetRow(Comment comment)
        {
            return new CommentListRow(comment);
        }

        protected override void EmptyDataSetBehavior()
        {
            AddRow(new DefaultFixedRow { 
                Text = "No Comments"
            });
        }

        protected override void BeforeAddingRows()
        {
            AddRow(new QuestionListRow(_post) { RichTags = true });
            AddRow(new WebViewWithCodeRow { Html = _post.body });
            AddHeaderRow("Comments");
        }
	}
}
