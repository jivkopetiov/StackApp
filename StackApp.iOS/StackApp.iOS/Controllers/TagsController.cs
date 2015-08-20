using System;
using MonoTouch.UIKit;
using Stacklash.Core;
using MonoTouch.Foundation;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Stacklash.iOS
{
	public class TagsController : UICollectionViewController
	{
        private int page = 1;
        private List<Tag> _tags;
        private bool _hasMoreData;
        private bool _pagingActionInProgress;

        public TagsController() : base(new UICollectionViewFlowLayout())
        {
            Title = "Tags";
        }

        public async override void ViewDidLoad()
        {
            this.InitLeftButton();
            CollectionView.BackgroundColor = UIColor.White;   

            try {
                CollectionView.RegisterClassForCell(typeof(TagCell), new NSString("tagcell"));
                CollectionView.Delegate = new ViewDelegate(this);

                var response = await ServiceProxy.GetAllTagsByPopularity(page, 100);
                _tags = response.items;
                _hasMoreData = response.has_more;
                CollectionView.ReloadData();
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }

        private class ViewDelegate : UICollectionViewDelegateFlowLayout {

            private TagsController _c;

            public async override void Scrolled(UIScrollView scrollView)
            {
                if (scrollView.Dragging && _c._hasMoreData)
                {
                    float threshold = scrollView.ContentSize.Height - scrollView.Bounds.Height;
                    if (scrollView.ContentOffset.Y > threshold - 160 && !_c._pagingActionInProgress)
                    {
                        Console.WriteLine("starting infinite loading");
                        _c._pagingActionInProgress = true;
                        BaseResponse<Tag> response = null;
                        try {
                            response = await ServiceProxy.GetAllTagsByPopularity(_c.page + 1, 100);
                        }
                        catch (Exception ex) {
                            Console.WriteLine(ex);
                            NSTimer.CreateScheduledTimer(0.6f, delegate {
                                _c._pagingActionInProgress = false;
                            });
                            return;
                        }

                        _c.page++;
                        _c._hasMoreData = response.has_more;
                        int currentCount = _c._tags.Count;
                        _c._tags.AddRange(response.items);
                        _c.CollectionView.PerformBatchUpdates(delegate {
                            var indexPaths = Enumerable.Range(currentCount, response.items.Count).Select(r => NSIndexPath.FromItemSection(r, 0));
                            _c.CollectionView.InsertItems(indexPaths.ToArray());
                        }, null);

                        NSTimer.CreateScheduledTimer(0.6f, delegate {
                            _c._pagingActionInProgress = false;
                        });
                    }
                }
            }

            public ViewDelegate(TagsController c)
            {
                _c = c;
            }

            public override SizeF GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
            {
                var tag = _c._tags[indexPath.Item];
                string name = " " + tag.name + " ";
                float width = collectionView.StringSize(name, UIFont.SystemFontOfSize(13), new SizeF(160, 20)).Width;
                return new SizeF(width, 27);
            }

            public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout, int section)
            {
                return new UIEdgeInsets(10, 10, 10, 10);
            }

            public override bool ShouldHighlightItem(UICollectionView collectionView, NSIndexPath indexPath)
            {
                return true;
            }

            public override void ItemHighlighted(UICollectionView collectionView, NSIndexPath indexPath)
            {
                var tag = _c._tags[indexPath.Item];
                Nav.Push(_c, new QuestionsByTagController(tag.name));
            }
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            var cell = (TagCell)collectionView.DequeueReusableCell(new NSString("tagcell"), indexPath);
            var tag = _tags[indexPath.Item];
            cell.ReloadUI(tag);

            return cell;
        }

        public override int NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override int GetItemsCount(UICollectionView collectionView, int section)
        {
            if (_tags == null)
                return 0;

            int count = _tags.Count;
            return count;
        }

        public class TagCell : UICollectionViewCell {

            private UILabel _label;

            [Export ("initWithFrame:")]
            public TagCell (RectangleF frame) : base (frame)
            {
                _label = new UILabel();
                _label.Font = UIFont.SystemFontOfSize(13);
                _label.BackgroundColor = Colors.FromHex("c4dae9");
                _label.TextAlignment = UITextAlignment.Center;
                _label.TextColor = Colors.FromHex("4a6b82");
                _label.UserInteractionEnabled = true;
                BackgroundView = new UIView();
                BackgroundView.UserInteractionEnabled = true;
                BackgroundView.Add(_label);
            }

            public void ReloadUI(Tag tag) {
                _label.Text = " " + tag.name + " ";
                _label.Frame = BackgroundView.Frame;
            }
        }
	}
}
