using System;
using Abilitics.iOS;
using Stacklash.Core;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SDWebImage;

namespace Stacklash.iOS
{
	public class SiteChooserController : RichListController
	{
        private bool _initial;
        private UIImage placeholder = UIImage.FromFile("noimage.png");

        public SiteChooserController(bool initial)
        {
            Title = "Choose site";
            EnableSearchBar = true;
            _initial = initial;
        }

        public async override void ViewDidLoad()
        {
            try {
                if (!_initial)
                    this.InitLeftButton();

                LongRunning = true;
                base.ViewDidLoad();

                var sites = await ServiceProxy.GetAllSites();

                foreach (var site in sites.items)
                {
                    var captured = site;

                    AddRow(new SubtitleDynamicRow {
                        Text = site.NameDecoded,
                        Details = site.AudienceDecoded,
                        ImageGetter = () => placeholder,
                        Action = delegate {
                            Config.CurrentSite = captured;
                            ServiceProxy.InitSite(captured.api_site_parameter);

                            if (_initial)
                                Nav.BuildDeck();
                            else {
                                Nav.LeftMenu.ReloadAllData();
                                Nav.CloseLeftAndOpen(new QuestionsLatestController());
                            }
                        },
                        AfterGetCellInit = cell => {
                            cell._image.SetImage(new NSUrl(captured.icon_url), placeholder, 
                                                    delegate(UIImage image, NSError error, SDImageCacheType cacheType)
                                                    {
                                cell._image.Image = image;
                            });
                        }
                    });
                }

                ForceReloadData();

                _tableView.ScrollToRow(NSIndexPath.FromRowSection(0, 0), UITableViewScrollPosition.Top, false);
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }
	}
}

