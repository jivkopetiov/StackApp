using System;
using Abilitics.iOS;
using MonoTouch.UIKit;
using System.Drawing;
using Stacklash.Core;
using MonoTouch.Foundation;
using System.Web;

namespace Stacklash.iOS
{
    public class CommentListRow : CustomContentRow
    {
        private readonly Comment _comment;
        private string _body;
        public static readonly UIFont HeadingFont = UIFont.BoldSystemFontOfSize(17);
        public static readonly UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);

        public CommentListRow(Comment comment)
        {
            _comment = comment;
            _body = _comment.BodyDecoded;
            if (_body.IsNotNullOrEmpty())
            {
                _body = BclEx.RemoveHtmlTags(_body);
                _body = _body.Trim();
                _body = _body.CollapseWhitespace();
            }
        }

        private class InnerCell : UITableViewCell {
            private UILabel _text;
            private UILabel _details;
            private UILabel _details2;
            private UserLabel _author;

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
            }

            public void ReloadUI(CommentListRow row, UITableView tableView) {

                var comment = row._comment;

                _text.Text = row._body;
                _details.Text = BclEx.OffsetFromNow(comment.creation_date) + " by";
                _details2.Text = string.Format("{0}\uD83D\uDC4D", comment.score);
                _author.SetData(row._controller, comment.owner);

                float titleHeight = row.GetHeightTitle(tableView);

                _text.Frame = new RectangleF(6, 0, tableView.Frame.Width - 24, titleHeight);
                _details.Frame = new RectangleF(6, titleHeight, _details.StringSize(_details.Text, _details.Font).Width, 20);
                _author.Frame = new RectangleF(_details.Frame.X + _details.Frame.Width + 4, titleHeight, _author.StringSize(_author.Text, _author.Font).Width, 20);
                _details2.Frame = new RectangleF(6, 20 + titleHeight, tableView.Frame.Width - 24, 20);
            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (InnerCell)tableView.DequeueReusableCell ("commentlistcell");

            if (cell == null)
                cell = new InnerCell("commentlistcell");

            cell.ReloadUI(this, tableView);

            return cell;
        }

        public float GetHeightTitle(UITableView tableView) {

            var size2 = tableView.StringSize (_body ?? "", HeadingFont, 
                                              new SizeF(tableView.Frame.Width - 24, 280));

            return size2.Height + 2;
        }

        public override float GetHeight (UITableView tableView)
        {
            return Math.Max (GetHeightTitle(tableView) + 42, 44);
        }
    }
}

