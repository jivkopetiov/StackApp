using System;
using Abilitics.iOS;
using Stacklash.Core;
using System.Web;

namespace Stacklash.iOS
{
	public class TopTagsForUserController : StackBaseListController<TopTag>
	{
        private int _userId;

        public TopTagsForUserController(int userId)
        {
            Title = "Top Tags";
            _userId = userId;
        }

        protected override System.Threading.Tasks.Task<BaseResponse<TopTag>> GetStackData()
        {
            return ServiceProxy.GetTopTagsForUser(_userId, _page);
        }

        protected override ContentRow GetRow(TopTag tag)
        {
            return new SubtitleFixedRow
            {
                Text = HttpUtility.HtmlDecode(tag.tag_name),
                Details = tag.answer_score + " rep, " + tag.answer_count + " answers",
                NavController = () => new TopQuestionsByUserAndTagController(_userId, tag.tag_name)
            };
        }
	}
}
