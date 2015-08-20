using System;
using Abilitics.iOS;
using Stacklash.Core;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Stacklash.iOS
{
	public class LeftMenuController : RichListController
	{
        public override void ViewDidLoad()
        {
            IsPlainTable = true;
            LongRunning = true;
            base.ViewDidLoad();

            ReloadAllData();
        }

        public override void ViewDidAppear(bool animated)
        {
            View.SetWidth (Nav.SettingsWidth);
            NavigationController.View.SetWidth(Nav.SettingsWidth);

            base.ViewDidAppear(animated);
        }

        public void ReloadAllData()
        {
            try {
                ClearAllRows();

                var site = Config.CurrentSite;
                var me = Config.CurrentUser;
                string accessToken = Config.AccessToken;

                if (me != null)
                    AddRow(new UserListRow(me) { Action = delegate { Nav.CloseLeftAndOpen(new UserProfileController(me)); } });

                AddRow(new SubtitleFixedRow
                       {
                    Text = "You are in " + site.NameDecoded,
                    Details = "tap to change",
                    Action = delegate { Nav.CloseLeftAndOpen(new SiteChooserController(initial:false)); }
                });

                AddRow(new DefaultFixedRow
                       { 
                    Text = "Browse Questions",
                    Action = delegate { Nav.CloseLeftAndOpen(new QuestionsLatestController()); }
                });

                AddRow(new DefaultFixedRow
                       { 
                    Text = "Browse Tags",
                    Action = delegate { Nav.CloseLeftAndOpen(new TagsController()); }
                });

                AddRow(new DefaultFixedRow
                       { 
                    Text = "Browse Users",
                    Action = delegate { Nav.CloseLeftAndOpen(new UsersController()); }
                });

                if (accessToken.IsNotNullOrEmpty())
                {
                    AddRow(new DefaultFixedRow { 
                        Text = "Favorites",
                        Action = delegate { Nav.CloseLeftAndOpen(new FavoritesByUserController(me.user_id, false)); }
                    });

                    AddRow(new DefaultFixedRow { 
                        Text = "Notifications",
                        Action = delegate { Nav.CloseLeftAndOpen(new NotificationsController()); }
                    });

                    AddRow(new DefaultFixedRow { 
                        Text = "Inbox",
                        Action = delegate { Nav.CloseLeftAndOpen(new InboxController()); }
                    });
                }

                AddRow(new DefaultFixedRow { 
                    Text = "Site Info",
                    Action = delegate { Nav.CloseLeftAndOpen(new SiteInfoController(site.api_site_parameter)); }
                });

                AddRow(new DefaultFixedRow {
                    Text = "History",
                    Action = delegate { Nav.CloseLeftAndOpen(new QuestionsHistoryController()); }
                });

                AddRow(new DefaultFixedRow {
                    Text = "About Stacklash",
                    Action = delegate { Nav.CloseLeftAndOpen(new AboutController()); }
                });

                AddHeaderRow(" ");

                if (accessToken.IsNotNullOrEmpty())
                {
                    AddRow(new DefaultFixedRow
                    { 
                        Text = "Logout",
                        Action = delegate { 

                            foreach (NSHttpCookie cookie in NSHttpCookieStorage.SharedStorage.Cookies)
                                 NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);

                            NSUrlCache.SharedCache.RemoveAllCachedResponses();

                            Config.AccessToken = null;
                            Config.CurrentUser = null;
                            Nav.BuildDeck();
                        }
                    }); 
                }
                else
                {
                    AddRow(new DefaultFixedRow
                    { 
                        Text = "Login",
                        Action = delegate { Nav.CloseLeftAndOpen(new LoginController()); }
                    });
                }

                ForceReloadData();
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }
	}
}

