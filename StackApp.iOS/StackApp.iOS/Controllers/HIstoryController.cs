using System;
using System.Linq;
using Stacklash.Core;
using System.Collections.Generic;
using MonoTouch.UIKit;

namespace Stacklash.iOS
{
    public class HistoryControllerBase<T> : StackBaseListController<T>
    {
        public override void ViewDidLoad()
        {
            this.InitLeftButton();
            Title = "History";
            IsPlainTable = true;
            LongRunning = true;

            var tabs = new UISegmentedControl(new[] { "Questions", "Users" });

            if (typeof(T) == typeof (Post))
                tabs.SelectedSegment = 0;
            else 
                tabs.SelectedSegment = 1;

            tabs.ControlStyle = UISegmentedControlStyle.Bar;
            AddBottomToolbar(new [] { new UIBarButtonItem(tabs) });

            tabs.ValueChanged += delegate {
                if (tabs.SelectedSegment == 1)
                    Nav.CloseLeftAndOpenNoAnimation(new UsersHistoryController());
                else 
                    Nav.CloseLeftAndOpenNoAnimation(new QuestionsHistoryController());
            };

            base.ViewDidLoad();
        }
    }

    public class QuestionsHistoryController : HistoryControllerBase<Post> {

        protected override System.Threading.Tasks.Task<BaseResponse<Post>> GetStackData()
        {
            var items = Database.GetHistory(HistoryItemType.Question).Result;
            return ServiceProxy.GetQuestionsByIds(items.Select(i => i.ItemId).ToList());
        }

        protected override Abilitics.iOS.ContentRow GetRow(Post item)
        {
            return new QuestionListRow(item)
            {
                RichTags = true,
                NavController = () => new QuestionDetailsController(item)
            };
        }
    }

    public class UsersHistoryController : HistoryControllerBase<ShallowUser> {

        protected override System.Threading.Tasks.Task<BaseResponse<ShallowUser>> GetStackData()
        {
            var items = Database.GetHistory(HistoryItemType.User).Result;
            return ServiceProxy.GetUsersByIds(items.Select(i => i.ItemId).ToList());
        }

        protected override Abilitics.iOS.ContentRow GetRow(ShallowUser user)
        {
            return new UserListRow(user);
        }
    }


}

