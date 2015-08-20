using System;
using System.Linq;
using Stacklash.Core;
using Abilitics.iOS;
using MonoTouch.UIKit;

namespace Stacklash.iOS
{
	public class TimelineController : StackBaseListController<Timeline>
	{
        private int _userId;

        public TimelineController(int userId)
        {
            Title = "Timeline";
            _userId = userId;
        }

        protected override System.Threading.Tasks.Task<BaseResponse<Timeline>> GetStackData()
        {
            return ServiceProxy.GetUserTimeline(_userId, _page);
        }

        protected override ContentRow GetRow(Timeline item)
        {
            string text = "";
            text += item.TitleDecoded;
            if (!item.DetailDecoded.IsNotNullOrEmpty()) {
                if (!text.IsNotNullOrEmpty())
                    text += ", ";

                text += ", " + item.DetailDecoded;
            }

            var row = new TitleDynamicRow {
                Text = text,
                Details = item.timeline_type + " " + BclEx.OffsetFromNow(item.creation_date),

            };

            var clickableTypes = new[] { "answered", "commented", "asked", "accepted", "revision" };

            if (clickableTypes.Contains(item.timeline_type))
            {
                if (item.post_type == "question")
                    row.NavController = () => new QuestionDetailsController(item.post_id);
                else if (item.post_type == "answer")
                    row.NavController = () => new AnswerDetailsController(item.post_id);

                row.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }

            return row;
        }
	}
}
