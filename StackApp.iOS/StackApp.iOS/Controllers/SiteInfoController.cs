using System;
using MonoTouch.UIKit;
using Abilitics.iOS;
using Stacklash.Core;
using System.Drawing;
using System.Linq;
using SDWebImage;

namespace Stacklash.iOS
{
	public class SiteInfoController : RichListController
	{
        private string _siteName;

		public SiteInfoController (string siteName)
		{
            _siteName = siteName;
		}

        public async override void ViewDidLoad()
        {
            try {
                this.InitLeftButton();
                LongRunning = true;
                Title = "Site Info";

                base.ViewDidLoad();

                var site = await ServiceProxy.GetSiteInfo(_siteName);

                var statistics = site.items[0];

                var imageView = new UIImageView(new RectangleF(0, 0, View.Frame.Width, 60));
                imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                imageView.SetImage(new MonoTouch.Foundation.NSUrl(statistics.site.logo_url));

                AddTableHeader(imageView);

                AddRow(new SubtitleDynamicRow { 
                    Text = statistics.site.NameDecoded,
                    Details = statistics.site.AudienceDecoded
                });

                AddSubtitleFixedRow("Site Url", statistics.site.site_url);
                AddValue1FixedRow("Launch Date", BclEx.UnixToDate(statistics.site.launch_date).ToString("d MMM yyyy"));
                AddRow(new DefaultFixedRow { 
                    Accessory = UITableViewCellAccessory.DisclosureIndicator,
                    Text = "View Moderators", 
                    NavController = () => new ModeratorsController()
                });

                AddHeaderRow("Site statistics");
                AddValue1FixedRow("Total Questions", statistics.total_questions.ToString());
                AddValue1FixedRow("Unanswered Questions", statistics.total_unanswered.ToString());
                AddValue1FixedRow("Total Answers", statistics.total_answers.ToString());
                AddValue1FixedRow("Accepted Answers", statistics.total_accepted.ToString());
                AddValue1FixedRow("Total Comments", statistics.total_comments.ToString());
                AddValue1FixedRow("Total Users", statistics.total_users.ToString());
                AddValue1FixedRow("New Active Users", statistics.new_active_users.ToString());
                AddValue1FixedRow("Total Votes", statistics.total_votes.ToString());
                AddValue1FixedRow("Total Badges", statistics.total_badges.ToString());
                AddValue1FixedRow("Badges per minute", statistics.badges_per_minute.ToString());
                AddValue1FixedRow("Questions per minute", statistics.questions_per_minute.ToString());
                AddValue1FixedRow("Answers per minute", statistics.answers_per_minute.ToString());

                AddHeaderRow("Developer info");
                AddValue1FixedRow("API Revision", statistics.api_revision);
                AddValue1FixedRow("API Site Param", statistics.site.api_site_parameter);
                AddSubtitleFixedRow("Markdown Extensions", statistics.site.markdown_extensions.JoinStrings(", "));

                if (statistics.site.aliases != null && statistics.site.aliases.Any())
                    AddSubtitleDynamicRow("Aliases", statistics.site.aliases.JoinStrings(", "));

                AddValue1FixedRow("Site State", statistics.site.site_state);
                AddValue1FixedRow("Site Type", statistics.site.site_type);

                ForceReloadData();
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }
	}
}
