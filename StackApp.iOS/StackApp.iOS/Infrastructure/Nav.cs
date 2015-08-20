using System;
using MonoTouch.UIKit;
using Stacklash.Core;
using System.Drawing;

namespace Stacklash.iOS
{
    public static class Nav
    {
        public static ViewDeckController Deck;
        private static UIWindow _window;

        public static UIViewController Center { get {
            return (Deck.CenterController as UINavigationController).TopViewController;
        }}

        public static LeftMenuController LeftMenu { get {
                return (Deck.LeftController as UINavigationController).TopViewController as LeftMenuController;
            }}

        public static readonly float SettingsWidth = 250;
        public static readonly float LeftLedge = 70;

        public static void Init(UIWindow window)
        {
            _window = window;
        }

        public static void BuildDeck() {

            var site = Config.CurrentSite;

            if (site == null)
            {
                var center = new UINavigationController(new SiteChooserController(initial: true));
                Deck = new ViewDeckController(center);
            }
            else {
                ServiceProxy.InitSite(site.api_site_parameter);
                var center = new UINavigationController(new QuestionsLatestController());
                var left = new UINavigationController(new LeftMenuController());
                Deck = new ViewDeckController(center, left);
            }

            //Deck.PanningMode = ViewDeckPanningMode.FullViewPanning;
            Deck.CenterInteractivity = CenterHiddenInteractivity.NotUserInteractiveWithTapToClose;
            Deck.LeftLedge = LeftLedge;

            _window.RootViewController = Nav.Deck;

            //CloseLeftAndOpen(new UsersController());

            //CloseLeftAndOpen(new TimelineController(22656));

            //CloseLeftAndOpen(new QuestionsLatestController());

            //CloseLeftAndOpen(new QuestionBodyController(await ServiceProxy.GetPostById(17209882)));

            //CloseLeftAndOpen(new QuestionDetailsController(901115));

            //CloseLeftAndOpen(new UserProfileController(new User { user_id = 22656 }));

            //CloseLeftAndOpen(new TagsController());
        }

        public static void CloseLeftAndOpen (UIViewController controller)
        {
            Deck.CenterController = new UINavigationController (controller);
            Deck.CloseLeftView (true);
        }

        public static void CloseLeftAndOpenNoAnimation (UIViewController controller)
        {
            Deck.CenterController = new UINavigationController (controller);
            Deck.CloseLeftView (false);
        }

        public static void PopToRoot(UIViewController current) {
            current.NavigationController.PopToRootViewController (true);
        }

        public static void PopToRootAndPush(UIViewController current, UIViewController next) {
            var root = current.NavigationController.ViewControllers [0];
            current.NavigationController.PopToRootViewController (false);
            root.NavigationController.PushViewController (next, true);
        }

        public static void PopAndPush(UIViewController current, UIViewController next) {
            var previous = current.NavigationController.ViewControllers [current.NavigationController.ViewControllers.Length - 2];
            current.NavigationController.PopViewControllerAnimated(false);
            previous.NavigationController.PushViewController (next, true);
        }

        public static void Pop(UIViewController current) {
            if (current.NavigationController.ViewControllers.Length < 2)
                return; 

            current.NavigationController.PopViewControllerAnimated (true);
        }

        public static void Push(UIViewController current, UIViewController next) {
            current.NavigationController.PushViewController (next, true);
        }

        public static void Modal(UIViewController current, UIViewController next) {
            var nav = new UINavigationController (next);
            current.PresentViewController (nav, true, null);
        }

        public static void DismissModal(UIViewController current) {
            current.DismissViewController (true, null);
        }

        public static void DismissModalAndOpen(UIViewController current, UIViewController next) {
            current.DismissViewController (true, delegate {
                Deck.CenterController = new UINavigationController (next);
            });
        }

        public static void PushNoAnimation(UIViewController current, UIViewController next) {
            current.NavigationController.PushViewController (next, false);
        }

        public static void InitLeftButton(this UIViewController c) {
            var button = UIButton.FromType (UIButtonType.Custom);
            button.Frame = new RectangleF (0, 0, 32, 32);
            button.SetBackgroundImage (UIImage.FromFile ("three-line-menu.png"), UIControlState.Normal);
            button.TouchUpInside += delegate {
                Deck.OpenLeftView(true);
            };

            c.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(button);
        }
    }
}

