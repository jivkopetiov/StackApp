using System;
using Abilitics.iOS;
using Stacklash.Core;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace Stacklash.iOS
{
    public class InboxRow : CustomContentRow
    {
        public InboxRow(InboxItem item)
        {
            _item = item;
        }

        private InboxItem _item;
        public string Byline;
        public string BadgeText;
        public UIFont HeadingFont = UIFont.BoldSystemFontOfSize(14);
        public UIFont DetailsFont = UIFont.SystemFontOfSize(13);

        public bool HasImage { get { return ImageUrl != null || ImageGetter != null ; } }

        private class SpotNewsCell : UITableViewCell {
            private UILabel _title;
            private UILabel _details;
            private UILabel _details2;
            public UIImageView _image;

            public SpotNewsCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
            {
                _title = new UILabel();
                _title.BackgroundColor = UIColor.Clear;
                _title.Lines = 0;
                Add (_title);

                _details = new UILabel();
                _details.BackgroundColor = UIColor.Clear;
                _details.TextColor = UIColor.Gray;
                _details.Lines = 0;
                Add (_details);

                _details2 = new UILabel();
                _details2.Font = UIFont.ItalicSystemFontOfSize(12);
                _details2.BackgroundColor = UIColor.Clear;
                _details2.TextColor = UIColor.Gray;
                Add (_details2);

                _image = new UIImageView();
                _image.ContentMode = UIViewContentMode.ScaleAspectFit;
                Add (_image);
            }

            public void ReloadUI(InboxRow row, UITableView tableView) {

                var item = row._item;
                _title.Text = item.TitleDecoded;
                _details.Text = item.BodyDecoded;
                _details2.Text = item.item_type.Replace("_", " ") + " " + BclEx.OffsetFromNow(item.creation_date);

                _title.Font = row.HeadingFont;
                _details.Font = row.DetailsFont;

                if (!string.IsNullOrEmpty(row.Details))
                    _details.Text = row.Details;

                float titleHeight = row.GetHeightTitle(tableView);
                float detailsHeight = row.GetHeightDetails(tableView);

                float leftOffset = row.HasImage ? 58 : 18;

                _details2.Frame = new RectangleF(leftOffset, 0, tableView.Frame.Width - leftOffset - 18, 20);
                _title.Frame = new RectangleF(leftOffset, 20, tableView.Frame.Width - leftOffset - 18, titleHeight);
                _details.Frame = new RectangleF(leftOffset, titleHeight + 20, tableView.Frame.Width - leftOffset - 18, detailsHeight);


                if (row.HasImage)
                    _image.Frame = new RectangleF(4, 4, 50, row.GetHeight (tableView) - 8);

                //if (row.ImageGetter != null)
                //    row.SetImage (this, row.ImageGetter());
            }

            private static UIView DecreaseWidth (UIView view, int pixels)
            {
                var oldFrame = view.Frame;
                view.Frame = new RectangleF (oldFrame.X, oldFrame.Y, oldFrame.Width - pixels, oldFrame.Height);
                return view;
            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (SpotNewsCell)tableView.DequeueReusableCell ("inboxrow");
            
            if (cell == null)
                cell = new SpotNewsCell("inboxrow");
            
            cell.ReloadUI(this, tableView);
            
            return cell;
        }
        
        public override void SetImage (UITableViewCell cell, UIImage image)
        {
            ((SpotNewsCell)cell)._image.Image = image;
        }
        
        public float GetHeightDetails(UITableView tableView) {

            if (string.IsNullOrEmpty (_item.BodyDecoded))
                return 0;

            float leftOffset = HasImage ? 58 : 18;
            
            var size = tableView.StringSize (_item.BodyDecoded, DetailsFont, 
                                             new SizeF(tableView.Frame.Width - leftOffset - 18, 50));
            
            return size.Height;
        }
        
        public float GetHeightTitle(UITableView tableView) {
            
            float leftOffset = HasImage ? 58 : 18;
            
            var size2 = tableView.StringSize (_item.TitleDecoded, HeadingFont, 
                                              new SizeF(tableView.Frame.Width - leftOffset - 18, 60));
            
            return size2.Height;
        }
        
        public override float GetHeight (UITableView tableView)
        {
            return Math.Max (GetHeightTitle(tableView) + GetHeightDetails (tableView) + 26, 44);
        }
    }
}

