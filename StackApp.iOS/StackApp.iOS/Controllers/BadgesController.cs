using System;
using Abilitics.iOS;
using Stacklash.Core;
using System.Threading.Tasks;

namespace Stacklash.iOS
{
    public class BadgesController : StackBaseListController<Badge>
    {
        private int _userId;

        public BadgesController(int userId)
        {
            Title = "Badges";
            _userId = userId;
        }

        protected override Task<BaseResponse<Badge>> GetStackData()
        {
            return ServiceProxy.GetUserBadges(_userId, BadgeSortBy.awarded, _page);
        }

        protected override ContentRow GetRow(Badge badge)
        {
            string text = "";
            if (badge.award_count > 1)
                text = badge.name + " " + "x " + badge.award_count;
            else
                text = badge.name;

            return new SubtitleDynamicRow
            {
                Text = text,
                Details = badge.rank + ", " + badge.description
            };
        }
    }
}
