using System;
using MonoTouch.UIKit;
using Abilitics.iOS;
using Stacklash.Core;
using System.Web;
using System.Drawing;
using System.Linq;

namespace Stacklash.iOS
{
	public class UserProfileController : RichListController
	{
        private ShallowUser _shallowUser;
        private UIView _container;
        private UIImageView _personImage;
        private UILabel _personName;

        public UserProfileController(ShallowUser shallowUser)
        {
            Title = "Profile";
            NavigationItem.BackBarButtonItem = new UIBarButtonItem(shallowUser.NameDecoded, UIBarButtonItemStyle.Bordered, null);
            _shallowUser = shallowUser;
        }

        public async override void ViewDidLoad()
        {
            try {
                LongRunning = true;
                base.ViewDidLoad();

                AddTableHeader(InitializeHeader());

                var response = await ServiceProxy.GetUserById(_shallowUser.user_id);
                var user = response.items.First();

                AddValue1FixedRow("Reputation", user.reputation.ToString());

                if (user.age > 0)
                    AddValue1FixedRow("Age", user.age.ToString());

                AddRow(new Value1FixedRow { 
                    Text = "Questions",
                    Details = user.question_count.ToString(),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator,
                    NavController = () => new QuestionsByUserController(user.user_id)
                });

                AddRow(new Value1FixedRow { 
                    Text = "Answers",
                    Details = user.answer_count.ToString(),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator,
                    NavController = () => new AnswersForUserController(user.user_id)
                });

                AddRow(new DefaultFixedRow {
                    Text = "Badges",
                    IsFixed = true,
                    NavController = () => new BadgesController(user.user_id),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator,
                    AfterGetCellInit = cell => {

                        float offset = 90;

                        if (user.badge_counts.silver > 999)
                            offset -= 4;

                        if (user.badge_counts.bronze > 999)
                            offset -= 4;

                        if (user.badge_counts.gold > 0) {
                            var label = new UILabel { Text = user.badge_counts.gold.ToString() };
                            var image = new UIImageView(UIImage.FromFile("badges/gold.png"));
                            cell.ContentView.Add(label);
                            cell.ContentView.Add(image);
                            label.Frame = new RectangleF(offset, 10, label.StringSize(label.Text, label.Font).Width, 24);
                            image.Frame = new RectangleF(label.Frame.X + label.Frame.Width, 10, 24, 24);
                            offset += label.Frame.Width + image.Frame.Width + 8;
                        }

                        if (user.badge_counts.silver > 0) {
                            var label = new UILabel { Text = user.badge_counts.silver.ToString() };
                            var image = new UIImageView(UIImage.FromFile("badges/silver.png"));
                            cell.ContentView.Add(label);
                            cell.ContentView.Add(image);
                            label.Frame = new RectangleF(offset, 10, label.StringSize(label.Text, label.Font).Width, 24);
                            image.Frame = new RectangleF(label.Frame.X + label.Frame.Width, 10, 24, 24);
                            offset += label.Frame.Width + image.Frame.Width + 8;
                        }

                        if (user.badge_counts.bronze > 0) {
                            var label = new UILabel { Text = user.badge_counts.bronze.ToString() };
                            var image = new UIImageView(UIImage.FromFile("badges/bronze.png"));
                            cell.ContentView.Add(label);
                            cell.ContentView.Add(image);
                            label.Frame = new RectangleF(offset, 10, label.StringSize(label.Text, label.Font).Width, 24);
                            image.Frame = new RectangleF(label.Frame.X + label.Frame.Width, 10, 24, 24);
                            offset += label.Frame.Width + image.Frame.Width + 8;
                        }
                    }
                });

                AddRow(new DefaultFixedRow { 
                    Text = "Favorites",
                    NavController = () => new FavoritesByUserController(user.user_id, true),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                });

                AddRow(new DefaultFixedRow { 
                    Text = "Mentions",
                    NavController = () => new UserMentionsController(user),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                });

                AddRow(new DefaultFixedRow { 
                    Text = "Top Tags",
                    NavController = () => new TopTagsForUserController(user.user_id),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                });

                AddValue1FixedRow("Profile Created", BclEx.OffsetFromNow(user.creation_date));

                AddSubtitleFixedRow("Votes", string.Format("{0} \uD83D\uDC4D, {1} \uD83D\uDC4E", user.up_vote_count, user.down_vote_count));

                if (user.is_employee)
                    AddDefaultFixedRow("He is StackExchange employee");

                AddRow(new SubtitleFixedRow { 
                    Text = "Activity",
                    Details = "Last seen " + BclEx.OffsetFromNow(user.last_access_date),
                    NavController = () => new TimelineController(user.user_id),
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                });

                if (user.location.IsNotNullOrEmpty())
                    AddSubtitleDynamicRow("Location", user.location);

                AddValue1FixedRow("Profile Views", user.view_count.ToString());

                if (user.website_url.IsNotNullOrEmpty())
                    AddRow(new SubtitleFixedRow
                    {
                        Text = "Website", 
                        Details = user.website_url,
                        Accessory = UITableViewCellAccessory.DisclosureIndicator,
                        NavController = () => new WebBrowserController(user.website_url)
                    });

                string bio = user.about_me;
                if (bio.IsNotNullOrEmpty()) {
                    bio = HttpUtility.HtmlDecode(bio);
                    bio = BclEx.RemoveHtmlTags(bio);
                    bio = bio.Trim();
                    bio = bio.CollapseWhitespace();
                }

                if (bio.IsNotNullOrEmpty())
                {
                    AddRow(new SubtitleDynamicRow { 
                        Text = "About",
                        Details = bio,
                        MaxHeight = 80,
                        Accessory = UITableViewCellAccessory.DisclosureIndicator,
                        NavController = () => new UserBioController(user)
                    });
                }

                ForceReloadData();

                await Database.AddUserHistory(_shallowUser.user_id);
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }

        private UIView InitializeHeader() {
            _container = new UIView();
            _container.Frame = new RectangleF(0, 0, View.Frame.Width, IOS.IsIpad ? 170 : 112);

            _personImage = new UIImageView();
            _personImage.ContentMode = UIViewContentMode.ScaleAspectFit;

            _personName = new UILabel();
            _personName.BackgroundColor = UIColor.Clear;
            _personName.Font = UIFont.BoldSystemFontOfSize (19);

            _container.AddSubview(_personName);
            _container.AddSubview(_personImage);

            _personName.Text = _shallowUser.NameDecoded;
            _personImage.Image = RequestImageOrEmpty(_shallowUser.profile_image, delegate {
                _personImage.Image = RequestImageOrEmpty(_shallowUser.profile_image, null);
            });

            return _container;
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear(animated);

            float imageWidth = IOS.IsIpad ? 170 : 100;
            float leftPadding = IOS.IsIpad ? 44 : 4;
            float offsetWidth = View.Frame.Width - (IOS.IsIpad ? 220 : 120);

            _personImage.Frame = new RectangleF(leftPadding, 4, imageWidth, imageWidth);
            _personName.Frame = new RectangleF(imageWidth + leftPadding + 10, 24, offsetWidth, 25);
            _container.Frame = new RectangleF(0, 0, View.Frame.Width, IOS.IsIpad ? 170 : 112);
        }
	}
}
