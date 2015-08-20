using System;
using Abilitics.iOS;
using MonoTouch.UIKit;
using System.Drawing;
using Stacklash.Core;
using MonoTouch.Foundation;

namespace Stacklash.iOS
{
    public class AnswerListRow : CustomContentRow
    {
        private readonly Post _post;
        private string _body;
        public static readonly UIFont HeadingFont = UIFont.BoldSystemFontOfSize(17);
        public static readonly UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);
        private bool _isDetails;

        public AnswerListRow(Post post, bool isDetails)
        {
            _isDetails = isDetails;
            _post = post;

            _body = _post.BodyDecoded;
            if (_body.IsNotNullOrEmpty())
            {
                _body = BclEx.RemoveHtmlTags(_body);
                _body = _body.Trim();
                _body = _body.CollapseWhitespace();
            }
        }

        public bool HasImage { get { return ImageUrl != null || ImageGetter != null ; } }

        private class InnerCell : UITableViewCell {
            private UILabel _text;
            private UILabel _details;
            private UILabel _details2;
            private UserLabel _author;
            private UIImageView _tick;

            public InnerCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
            {
                _text = new UILabel();
                _text.BackgroundColor = UIColor.Clear;
                _text.Font = HeadingFont;
                _text.Lines = 0;
                Add (_text);

                _details = new UILabel();
                _details.BackgroundColor = UIColor.Clear;
                _details.Font = DetailsFont;
                _details.TextColor = UIColor.Gray;
                Add (_details);

                _details2 = new UILabel();
                _details2.Font = UIFont.ItalicSystemFontOfSize(12);
                _details2.BackgroundColor = UIColor.Clear;
                _details2.TextColor = UIColor.Gray;
                Add (_details2);

                _author = new UserLabel();
                _author.Font = DetailsFont;
                Add (_author);

                _tick = new UIImageView(UIImage.FromFile("tick2.png"));
                Add(_tick);
            }

            public void ReloadUI(AnswerListRow row, UITableView tableView) {

                var post = row._post;

                _text.Text = row._body;

                _details.Text = BclEx.OffsetFromNow(post.creation_date) + " by";
                _author.SetData(row._controller, post.owner);
                _details2.Text = string.Format("{0}\uD83D\uDC4D  {1} comments", post.score, post.comments != null ? post.comments.Count : 0);

                float titleHeight = row.GetHeightTitle(tableView);

                _text.Frame = new RectangleF(6, 0, tableView.Frame.Width - 24, titleHeight);
                _details.Frame = new RectangleF(6, titleHeight, _details.StringSize(_details.Text, _details.Font).Width, 20);
                _author.Frame = new RectangleF(_details.Frame.X + _details.Frame.Width + 4, titleHeight, _author.StringSize(_author.Text, _author.Font).Width, 20);
                _details2.Frame = new RectangleF(6, 20 + titleHeight, tableView.Frame.Width - 24, 20);

                if (post.is_accepted)
                    _tick.Frame = new RectangleF(_details2.StringSize(_details2.Text, _details2.Font).Width + 10, _details2.Frame.Y, 20, 20);
                else
                    _tick.Frame = RectangleF.Empty;

                if (row._isDetails)
                    _text.Frame = RectangleF.Empty;
            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (InnerCell)tableView.DequeueReusableCell ("answerlistcell");

            if (cell == null)
                cell = new InnerCell("answerlistcell");

            cell.ReloadUI(this, tableView);

            return cell;
        }

        public float GetHeightTitle(UITableView tableView) {

            if (_isDetails)
                return 4;

            var size2 = tableView.StringSize (_body ?? "", HeadingFont, 
                                              new SizeF(tableView.Frame.Width - 24, 80));

            return size2.Height + 2;
        }

        public override float GetHeight (UITableView tableView)
        {
            return Math.Max (GetHeightTitle(tableView) + 42, 44);
        }
    }
}

