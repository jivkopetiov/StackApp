using System;
using Abilitics.iOS;
using MonoTouch.UIKit;
using System.Drawing;
using Stacklash.Core;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Linq;

namespace Stacklash.iOS
{
    public class QuestionListRow : CustomContentRow
    {
        private readonly Post _post;
        public static readonly UIFont HeadingFont = UIFont.BoldSystemFontOfSize(17);
        public static readonly UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);
        public bool RichTags;

        public QuestionListRow(Post post)
        {
            _post = post;
        }

        public bool HasImage { get { return ImageUrl != null || ImageGetter != null ; } }

        private class InnerCell : UITableViewCell {
            private UILabel _text;
            private UILabel _details;
            private UserLabel _author;
            private UILabel _details2;
            private UIView _richDetails2;
            private UILabel _tags;

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

                _richDetails2 = new UIView();
                Add(_richDetails2);

                _tags = new UILabel();
                _tags.Font = UIFont.SystemFontOfSize(12);
                _tags.TextColor = Colors.FromHex("4a6b82");
                _tags.BackgroundColor = UIColor.Clear;
                Add (_tags);
            }

            public void ReloadUI(QuestionListRow row, UITableView tableView) {

                var post = row._post;

                _text.Text = post.TitleDecoded;
                _details.Text = BclEx.OffsetFromNow(post.creation_date) + " by";
                _author.SetData(row._controller, post.owner);
                _details2.Text = string.Format("{0}\uD83D\uDC4D  {1}\u2606  {2}\uD83D\uDC40  {3} answers  {4} comments", post.score, post.favorite_count, GetViewCount(post.view_count), post.answer_count, post.comments != null ? post.comments.Count : 0);

                if (post.tags != null)
                {
                    if (row.RichTags)
                        RichTags(post.tags, row);
                    else
                        PlainTags(post.tags);
                }

                float titleHeight = row.GetHeightTitle(tableView);

                _text.Frame = new RectangleF(6, 0, tableView.Frame.Width - 24, titleHeight);
                _details.Frame = new RectangleF(6, titleHeight, _details.StringSize(_details.Text, _details.Font).Width, 20);
                _author.Frame = new RectangleF(_details.Frame.X + _details.Frame.Width + 4, titleHeight, _author.StringSize(_author.Text, _author.Font).Width, 20);
                _details2.Frame = new RectangleF(6, 20 + titleHeight, tableView.Frame.Width - 24, 20);
                _richDetails2.Frame = new RectangleF(6, 40 + titleHeight, tableView.Frame.Width - 24, 20);
                _tags.Frame = new RectangleF(6, 40 + titleHeight, tableView.Frame.Width - 24, 20);
            }

            private string GetViewCount(int view_count)
            {
                if (view_count > 999)
                {
                    return (view_count / 1000) + "K";
                }
                else 
                    return view_count.ToString();
            }

            private void RichTags(List<string> tags, QuestionListRow row)
            {
                foreach (var v in _richDetails2.Subviews.Skip(5))
                {
                    v.RemoveFromSuperview();
                    v.Dispose();
                }

                int existing = _richDetails2.Subviews.Length;
                int existingCounter = 0;

                float offset = -2;
                foreach (string tag in tags)
                {
                    TagLabel label = null;

                    if (existingCounter < existing)
                    {
                        label = (TagLabel)_richDetails2.Subviews[existingCounter];
                        existingCounter++;
                    }
                    else
                    {
                        label = new TagLabel(row._controller);
                        _richDetails2.Add(label);
                    }

                    var size = label.StringSize(tag, label.Font);
                    label.Frame = new RectangleF(offset + 2, 0, size.Width + 4, 20);
                    label.Text = tag;
                    label.Hidden = false;

                    offset += size.Width + 8;
                }

                if (existingCounter < existing)
                {
                    for (int i = existingCounter; i < existing; i++)
                    {
                        _richDetails2.Subviews[i].Hidden = true;
                    }
                }
            }

            private void PlainTags(List<string> tags)
            {
                var tagAttributedString = new NSMutableAttributedString(" " + tags.JoinStrings("   ") + " ");
                int currentTagIndex = 0;
                foreach (string tag in tags)
                {
                    tagAttributedString.AddAttribute(UIStringAttributeKey.BackgroundColor, Colors.FromHex("c4dae9"), new NSRange(currentTagIndex, tag.Length + 2));
                    currentTagIndex += tag.Length + 3;
                }
                _tags.AttributedText = tagAttributedString;
            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (InnerCell)tableView.DequeueReusableCell ("questionlistcell");

            if (cell == null)
                cell = new InnerCell("questionlistcell");

            cell.ReloadUI(this, tableView);

            return cell;
        }

        public float GetHeightTitle(UITableView tableView) {

            var size2 = tableView.StringSize (_post.TitleDecoded, HeadingFont, 
                                              new SizeF(tableView.Frame.Width - 24, 60));

            return size2.Height + 2;
        }

        public override float GetHeight (UITableView tableView)
        {
            return Math.Max (GetHeightTitle(tableView) + 62, 44);
        }
    }
}

