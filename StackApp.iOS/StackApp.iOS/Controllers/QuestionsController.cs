using System;
using MonoTouch.UIKit;
using Abilitics.iOS;
using Stacklash.Core;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Linq;
using System.Drawing;
using MonoTouch.ObjCRuntime;
using System.Collections.Generic;

namespace Stacklash.iOS
{
    public class TopQuestionsByUserAndTagController : QuestionsBaseController {
        private int _userId;
        private string _tag;

        public TopQuestionsByUserAndTagController(int userId, string tag)
        {
            _tag = tag;
            _userId = userId;
        }

        protected override Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetTopQuestionsByUserAndTag(_userId, PostSortBy.activity, _tag, _page);
        }
    }

    public class QuestionsByUserController : QuestionsBaseController
    {
        private int _userId;

        public QuestionsByUserController (int userId)
        {
            Title = "Questions";
            _userId = userId;
        }

        protected override Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetQuestionsForUser(_userId, PostSortBy.votes, _page);
        }
    }

    public class FavoritesByUserController : QuestionsBaseController
    {
        private int _userId;
        private bool _pushing;

        public FavoritesByUserController (int userId, bool pushing)
        {
            Title = "Favorites";
            _userId = userId;
            _pushing = pushing;
        }

        public override void ViewDidLoad()
        {
            if (!_pushing)
                this.InitLeftButton();

            base.ViewDidLoad();
        }

        protected override Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetFavoritesForUser(_userId, PostSortBy.votes, _page);
        }
    }

    public class QuestionsByTagController : QuestionsBaseController {

        private string _tag;

        public QuestionsByTagController(string tag)
        {
            Title = "Tagged '{0}'".Fmt(tag);
            _tag = tag;
        }

        protected override Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetQuestions(PostFilter.normal, PostSortBy.creation, _page, tag: _tag);
        }
    }

    public class QuestionsLatestController : QuestionsBaseController {

        private PostFilter _postFilter;
        private PostSortBy _postSortBy;
        private List<KxMenuItem> _menuItems;

        public override void ViewDidLoad()
        {
            this.InitLeftButton();
            base.ViewDidLoad();

            _menuItems = new [] { 
                new KxMenuItem { Title = "Latest", Action = delegate { SwitchFilter("Latest", PostSortBy.creation, PostFilter.normal); } },
                new KxMenuItem { Title = "Hottest", Action = delegate { SwitchFilter("Hottest", PostSortBy.hot, PostFilter.normal); } },
                new KxMenuItem { Title = "Featured", Action = delegate { SwitchFilter("Featured", PostSortBy.activity, PostFilter.featured); } },
                new KxMenuItem { Title = "Unanswered", Action = delegate { SwitchFilter("Unanswered", PostSortBy.votes, PostFilter.unanswered); } },
                new KxMenuItem { Title = "Most Voted", Action = delegate { SwitchFilter("Most Voted", PostSortBy.votes, PostFilter.normal); } },
                new KxMenuItem { Title = "Most Active", Action = delegate { SwitchFilter("Most Active", PostSortBy.activity, PostFilter.normal); } }
            }.ToList();

            SwitchFilter("Latest", PostSortBy.creation, PostFilter.normal, isInitial:true);

            var action = new UIBarButtonItem(UIBarButtonSystemItem.Action, this, new Selector("displayLogoutMenu:event:"));

            var search = new UIBarButtonItem(UIBarButtonSystemItem.Search, SearchButtonClicked);

            NavigationItem.RightBarButtonItems = new[] { action, search };
        }

        private void SearchButtonClicked(object sender, EventArgs args) {
            NavigationController.SetNavigationBarHidden(true, true);
            var searchBar = new UISearchBar(new RectangleF(0, 0, _tableView.Frame.Width, 44));
            searchBar.SetShowsCancelButton (true, false);

            searchBar.CancelButtonClicked += delegate {
                searchBar.ResignFirstResponder ();
                _tableView.TableHeaderView = null;
                NavigationController.SetNavigationBarHidden(false, true);
            };

            searchBar.SearchButtonClicked += async delegate {
                searchBar.ResignFirstResponder();

                try {
                    var data = await ServiceProxy.GetSearchResults(searchBar.Text);
                    ClearAndPopulateData(data);
                }
                catch (Exception ex) {
                    this.UnhandledError(ex);
                }
            };

            _tableView.TableHeaderView = searchBar;

            searchBar.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (NavigationController.NavigationBarHidden)
                NavigationController.SetNavigationBarHidden(false, true);

            base.ViewWillDisappear(animated);
        }

        protected override Task<BaseResponse<Post>> GetStackData()
        {
            return ServiceProxy.GetQuestions(_postFilter, _postSortBy, _page);
        }

        [Export ("displayLogoutMenu:event:")]
        public void DisplayLogoutMenu (UIBarButtonItem barButton, UIEvent evt)
        {
            var touch = evt.AllTouches.ToArray<UITouch>().First();
            var frame = new RectangleF(touch.View.Frame.X, -27, touch.View.Frame.Width, touch.View.Frame.Height);
            KxMenuView.Show(View, frame, _menuItems);
        }

        private async void SwitchFilter(string title, PostSortBy postSortBy, PostFilter postFilter, bool isInitial = false) {
            Title = title;
            _postSortBy = postSortBy;
            _postFilter = postFilter;

            if (!isInitial)
                await ReloadAllData();

            foreach (var item in _menuItems)
            {
                if (item.Title == title)
                    item.ForeColor = UIColor.FromRGBA(47 / 255.0f, 112 / 255.0f, 225 / 255.0f, 1.0f);
                else 
                    item.ForeColor = null;
            }
        }
    }

    public abstract class QuestionsBaseController : StackBaseListController<Post>
	{
        protected override ContentRow GetRow(Post item)
        {
            return new QuestionListRow(item)
            {
                RichTags = true,
                NavController = () => new QuestionDetailsController(item)
            };
        }
	}
}
