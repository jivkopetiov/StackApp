using System;
using Abilitics.iOS;
using Stacklash.Core;
using System.Linq;

namespace Stacklash.iOS
{
	public class QuestionDetailsController : StackBaseListController<Post>
	{
        private Post _post;
        private int _postId;

        public QuestionDetailsController (Post post)
        {
            _post = post;
        }

        public QuestionDetailsController(int postId) {
            _postId = postId;
        }

        public async override void ViewDidLoad()
        {
            Title = "Question";
            PagingSectionIndex = 1;

            base.ViewDidLoad();

            if (_post == null) {
                var response = await ServiceProxy.GetPostById(_postId);
                _post = response.items.First();
            }
            else 
                _postId = _post.question_id;

            await Database.AddQuestionHistory(_postId);
        }

        protected override System.Threading.Tasks.Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetAnswersForQuestion(_postId, PostSortBy.votes, _page);
        }

        protected override void EmptyDataSetBehavior()
        {
            AddRow(new DefaultFixedRow { 
                Text = "No Answers"
            });
        }

        protected override ContentRow GetRow(Post answer)
        {
            return new AnswerListRow(answer, isDetails: false)
            {
                NavController = () => new AnswerDetailsController(answer)
            };
        }

        protected override void BeforeAddingRows()
        {
            AddRow(new QuestionListRow(_post)
                   {
                NavController = () => new QuestionBodyController(_post),
                RichTags = true
            });

            AddHeaderRow("Answers");
        }
	}
}
