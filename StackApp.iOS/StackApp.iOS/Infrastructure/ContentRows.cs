using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using System.Collections.Generic;

namespace Abilitics.iOS
{
	public class BadgeSettings {
		public string BadgeNumber;
		public UIColor BadgeColor;
		public UIColor BadgeColorHighlighted;
		public string BadgeNumber2;
		public UIColor BadgeColor2;
		public UIColor BadgeColorHighlighted2;
	}
	
	public class DefaultFixedRow : StandardContentRow {
		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			return new StandardContentCell(UITableViewCellStyle.Default, Type);
		}
	}
	
	public class Value1FixedRow : StandardContentRow {
		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			return new StandardContentCell(UITableViewCellStyle.Value1, Type);
		}
	}
	
	public class Value2FixedRow : StandardContentRow {
		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			return new StandardContentCell(UITableViewCellStyle.Value2, Type);
		}
	}
	
	public class SubtitleFixedRow : StandardContentRow {

		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			return new StandardContentCell(UITableViewCellStyle.Subtitle, Type);
		}
	}

	public class ButtonRow : StandardContentRow {

		private UIButton button;

		public ButtonRow ()
		{
			TryToReuseCell = false;
		}

		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			var cell = new StandardContentCell(UITableViewCellStyle.Default, Type);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			button = UIButton.FromType (UIButtonType.Custom);

			button.SetTitle (Text, UIControlState.Normal);
			button.Font = UIFont.BoldSystemFontOfSize (16);
			button.BackgroundColor = BackgroundColor;
			button.TitleEdgeInsets = new UIEdgeInsets(0, 6, 0, 6);
			button.Layer.CornerRadius = 7.0f;
			button.SetTitleColor(TextColor, UIControlState.Normal);
			button.SizeToFit ();
			button.TouchUpInside += delegate {
				RowSelectedImpl (tableView);
			};

			if (Disable) {
				button.UserInteractionEnabled = false;
				button.Enabled = false;
				button.TitleLabel.Enabled = false;
			}

			float left = (tableView.Frame.Width - button.Frame.Width ) / 2 - 12;
			button.Frame = new RectangleF(left, 4, button.Frame.Width + 12, 36);

			cell.ContentView.Add (button);

			return cell;
		}

		protected override void FixedCommon (UITableView tableView, StandardContentCell cell, NSIndexPath indexPath)
		{
		}

		public override void RowSelected (UITableView tableView)
		{
		}
	}

	public class TwoButtonsRow : StandardContentRow {

		public Action Button1Action;
		public Action Button2Action;
		public string Button1Text;
		public string Button2Text;

		public TwoButtonsRow ()
		{
			TryToReuseCell = false;
		}

		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			var cell = new StandardContentCell(UITableViewCellStyle.Default, Type);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;


			var button1 = UIButton.FromType (UIButtonType.Custom);
			var button2 = UIButton.FromType (UIButtonType.Custom);

			button1.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			button2.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			button1.TitleLabel.TextAlignment = UITextAlignment.Center;
			button2.TitleLabel.TextAlignment = UITextAlignment.Center;
			button1.TitleLabel.Lines = 2;
			button2.TitleLabel.Lines = 2;

			button1.SetTitle (Button1Text, UIControlState.Normal);
			button1.Font = UIFont.BoldSystemFontOfSize (14);
			button1.BackgroundColor = BackgroundColor;
			button1.TitleEdgeInsets = new UIEdgeInsets(0, 6, 0, 6);
			button1.Layer.CornerRadius = 7.0f;
			button1.SetTitleColor(TextColor, UIControlState.Normal);
			button1.TouchUpInside += delegate {
				Button1Action();
			};


			button2.SetTitle (Button2Text, UIControlState.Normal);
			button2.Font = UIFont.BoldSystemFontOfSize (14);
			button2.BackgroundColor = BackgroundColor;
			button2.TitleEdgeInsets = new UIEdgeInsets(0, 6, 0, 6);
			button2.Layer.CornerRadius = 7.0f;
			button2.SetTitleColor(TextColor, UIControlState.Normal);
			button2.TouchUpInside += delegate {
				Button2Action();
			};

			button1.Frame = new RectangleF (0, 0, 120, 0);
			button2.Frame = new RectangleF (0, 0, 120, 0);

			const float spacingBetweenButtons = 16;
			float totalWidth = button1.Frame.Width + button2.Frame.Width + spacingBetweenButtons;
			float left = (tableView.Frame.Width - totalWidth) / 2;
			button1.Frame = new RectangleF(left, 4, button1.Frame.Width, 36);
			left += button1.Frame.Width + spacingBetweenButtons;
			button2.Frame = new RectangleF(left, 4, button2.Frame.Width, 36);

			cell.ContentView.Add (button1);
			cell.ContentView.Add (button2);

			return cell;
		}

		protected override void FixedCommon (UITableView tableView, StandardContentCell cell, NSIndexPath indexPath)
		{
		}

		public override void RowSelected (UITableView tableView)
		{
		}
	}

	public class CollapseRow : StandardContentRow {

		public UIColor CollapsedColor;
		public UIColor ExpandedColor;
		public UIColor TextCollapsedColor;
		public UIColor TextExpandedColor;

		public new UIColor BackgroundColor;
		public string ExpandedImagePath;
		public string CollapsedImagePath;

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (CollapseCell)tableView.DequeueReusableCell ("collapsecell");

			if (cell == null)
				cell = new CollapseCell (_controller, tableView, indexPath);

			cell.ReloadUI(this, tableView, indexPath);

			return cell;
		}
	}

	public class CollapseCell : StandardContentCell {

		private RichListController _controller;
		private UITableView _tableView;
		private UIButton imageButton;

		private UILabel detailsLabel;

		public CollapseCell (RichListController controller, UITableView tableView, NSIndexPath indexPath)
			: base(UITableViewCellStyle.Default, "collapsecell")
		{
			_controller = controller;
			_tableView = tableView;
			TextLabel.Font = controller._headingFont;
			SelectionStyle = UITableViewCellSelectionStyle.None;

			detailsLabel = new UILabel();
			detailsLabel.Layer.CornerRadius = 4f;
			detailsLabel.TextAlignment = UITextAlignment.Center;
			detailsLabel.TextColor = UIColor.White;
			detailsLabel.Font = UIFont.BoldSystemFontOfSize (13);
			detailsLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			AccessoryView = detailsLabel;

			imageButton = UIButton.FromType (UIButtonType.Custom);
			Add (imageButton);

			imageButton.TouchDown += delegate {
				ToggleCollapse();
			};

			BackgroundView = new UIView (Frame);
		}

		public void ReloadUI(CollapseRow row, UITableView tableView, NSIndexPath indexPath) {

			TextLabel.Text = row.Text;
			detailsLabel.Text = row.Details;
			detailsLabel.SizeToFit ();
			detailsLabel.Frame = new RectangleF (
				detailsLabel.Frame.X, detailsLabel.Frame.Y, detailsLabel.Frame.Width + 8, detailsLabel.Frame.Height);

			detailsLabel.BackgroundColor = row.BackgroundColor;

			if (row.CollapsedColor != null) {
				if (GetIsCollapsed(indexPath))
					BackgroundView.BackgroundColor = row.CollapsedColor;
				else 
					BackgroundView.BackgroundColor = row.ExpandedColor;
			}

			if (GetIsCollapsed (indexPath)) {
				if (row.CollapsedImagePath != null)
					ImageView.Image = UIImage.FromFile (row.CollapsedImagePath);

				if (row.TextCollapsedColor != null)
					TextLabel.TextColor = row.TextCollapsedColor;
			} else {
				if (row.ExpandedImagePath != null)
					ImageView.Image = UIImage.FromFile (row.ExpandedImagePath);

				if (row.TextExpandedColor != null)
					TextLabel.TextColor = row.TextExpandedColor;
			}

			if (row.CollapsedColor != null)
				TextLabel.BackgroundColor = UIColor.Clear;
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			imageButton.Frame = ImageView.Frame;
		}

		private void Expand(NSIndexPath indexPath, bool animate) {

			if (animate)
				_tableView.BeginUpdates ();

			var originalItems = _controller._originalItems[indexPath.Section].Item2.ToList ();

			_controller._items[indexPath.Section].Item2.AddRange (originalItems);
			var paths = new List<NSIndexPath>();

			for (int i = 1; i < originalItems.Count + 1; i++)
				paths.Add (NSIndexPath.FromRowSection (i, indexPath.Section));

			_tableView.InsertRows (paths.ToArray (),  UITableViewRowAnimation.Top);

			var row = _controller._collapseRows [indexPath.Section];
			if (row.ExpandedColor != null)
				BackgroundView.BackgroundColor = row.ExpandedColor;

			if (row.ExpandedImagePath != null)
				ImageView.Image = UIImage.FromFile (row.ExpandedImagePath);

			TextLabel.TextColor = row.TextExpandedColor;

			if (animate)
				_tableView.EndUpdates ();
		}

		private void Collapse(NSIndexPath indexPath, bool animate) {
			if (animate)
				_tableView.BeginUpdates ();

			var items = _controller._items[indexPath.Section].Item2;
			int itemsLength = items.Count;
			items.Clear();
			var paths = new List<NSIndexPath>();

			for (int i = 1; i < itemsLength + 1; i++) {
				var a = (NSIndexPath.FromRowSection (i, indexPath.Section));
				paths.Add (a);
			}

			_tableView.DeleteRows(paths.ToArray (),  UITableViewRowAnimation.Top);

			var row = _controller._collapseRows [indexPath.Section];
			if (row.CollapsedColor != null)
				BackgroundView.BackgroundColor = row.CollapsedColor;

			if (row.CollapsedImagePath != null)
				ImageView.Image = UIImage.FromFile (row.CollapsedImagePath);

			TextLabel.TextColor = row.TextCollapsedColor;

			if (animate)
				_tableView.EndUpdates ();
		}

		public void ForceExpand(NSIndexPath indexPath) {
			if (GetIsCollapsed (indexPath))
				Expand (indexPath, animate:false);
		}

		public void ForceCollapse(NSIndexPath indexPath) {
			if (!GetIsCollapsed (indexPath))
				Collapse (indexPath, animate:false);
		}

		public void ToggleCollapse() {

			var indexPath = _tableView.IndexPathForCell (this);

			bool isCollapsed = GetIsCollapsed (indexPath);

			if (isCollapsed)
				Expand (indexPath, animate:true);
			else 
				Collapse (indexPath, animate:true);
		}
			
		private bool GetIsCollapsed(NSIndexPath indexPath) {
			return _controller._items [indexPath.Section].Item2.Count == 0;
		}
	}

	public class TwoButtonsRoundedRectRow : StandardContentRow {

		public Action Button1Action;
		public Action Button2Action;
		private UIButton button1;
		private UIButton button2;
		public string Button1Text;
		public string Button2Text;

		protected override StandardContentCell CreateCell (UITableView tableView)
		{
			var cell = new StandardContentCell(UITableViewCellStyle.Default, Type);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			var backView = new UIView();
			backView.BackgroundColor = UIColor.Clear;
			cell.BackgroundView = backView;

			button1 = UIButton.FromType (UIButtonType.RoundedRect);
			button1.SetTitle (Button1Text, UIControlState.Normal);
			button1.Font = UIFont.BoldSystemFontOfSize (14);
			button1.SizeToFit ();
			button1.TouchUpInside += delegate {
				Button1Action();
			};

			button2 = UIButton.FromType (UIButtonType.RoundedRect);
			button2.SetTitle (Button2Text, UIControlState.Normal);
			button2.Font = UIFont.BoldSystemFontOfSize (14);
			button2.SizeToFit ();
			button2.TouchUpInside += delegate {
				Button2Action();
			};

			const float spacingBetweenButtons = 16;
			float totalWidth = button1.Frame.Width + button2.Frame.Width + spacingBetweenButtons;
			float left = (tableView.Frame.Width - totalWidth) / 2;
			button1.Frame = new RectangleF(left, 4, button1.Frame.Width, 36);
			left += button1.Frame.Width + spacingBetweenButtons;
			button2.Frame = new RectangleF(left, 4, button2.Frame.Width, 36);

			cell.ContentView.Add (button1);
			cell.ContentView.Add (button2);

			return cell;
		}

		protected override void FixedCommon (UITableView tableView, StandardContentCell cell, NSIndexPath indexPath)
		{
		}

		public override void RowSelected (UITableView tableView)
		{
		}
	}

	public class StandardContentCell : UITableViewCell {

		private StandardContentRow _row;

		public void SetRow(StandardContentRow row) {
			_row = row;
		}

		public StandardContentCell (UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
		{
		}

		private static UIView MoveRight (UIView view, float pixels)
		{
			var oldFrame = view.Frame;
			view.Frame = new RectangleF (oldFrame.X + pixels, oldFrame.Y, oldFrame.Width, oldFrame.Height);
			return view;
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			if (_row != null && _row.MarginLeft > 0) {
				MoveRight(ImageView, _row.MarginLeft);
				MoveRight(TextLabel, _row.MarginLeft);

				if (DetailTextLabel != null)
					MoveRight(DetailTextLabel, _row.MarginLeft);
			}

			if (_row != null && _row.ImageAlignedWidth.HasValue) {
				float w = ImageView.Frame.Size.Width;
				float desiredWidth = _row.ImageAlignedWidth.Value;

				ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

				if (w > desiredWidth) {
					float widthSub = w - desiredWidth;
					ImageView.Frame = new RectangleF(ImageView.Frame.X, ImageView.Frame.Y, desiredWidth, ImageView.Frame.Height);
					TextLabel.Frame = new RectangleF(TextLabel.Frame.X - widthSub, TextLabel.Frame.Y, TextLabel.Frame.Width + widthSub, TextLabel.Frame.Height);

					if (DetailTextLabel != null)
						DetailTextLabel.Frame = new RectangleF(DetailTextLabel.Frame.X - widthSub, DetailTextLabel.Frame.Y, DetailTextLabel.Frame.Width + widthSub, DetailTextLabel.Frame.Height);
				}
				else if (w < desiredWidth) {
					float widthSub = desiredWidth - w;
					ImageView.Frame = new RectangleF(ImageView.Frame.X, ImageView.Frame.Y, desiredWidth, ImageView.Frame.Height);
					TextLabel.Frame = new RectangleF(TextLabel.Frame.X + widthSub, TextLabel.Frame.Y, TextLabel.Frame.Width, TextLabel.Frame.Height);

					if (DetailTextLabel != null)
						DetailTextLabel.Frame = new RectangleF(DetailTextLabel.Frame.X + widthSub, DetailTextLabel.Frame.Y, DetailTextLabel.Frame.Width, DetailTextLabel.Frame.Height);
				}
			
			}
		}
	}

	public class StandardContentRow : ContentRow {
	
		public float MarginLeft { get; set; }
		public int? ImageAlignedWidth { get; set; }
		public UIColor TextColor { get; set; }
		public UIColor DetailsColor { get; set; }
		public UIFont TextFont { get; set; }
		public UIFont DetailsFont { get; set; }
		protected bool TryToReuseCell = true;

		public Action<StandardContentCell> AfterGetCellInit { get; set; }

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {

			StandardContentCell cell = null;

			if (TryToReuseCell)
				cell = tableView.DequeueReusableCell (Type) as StandardContentCell;

			if (cell == null)
				cell = CreateCell(tableView);

			cell.SetRow (this);

			FixedCommon (tableView, cell, indexPath);

			return cell;
		}

		protected virtual StandardContentCell CreateCell(UITableView tableView) {
			return null;
		}

		private static void ApplyBackgroundColor(StandardContentCell cell, UIColor color) {
			var view = new UIView ();
			view.BackgroundColor = color;
			cell.BackgroundView = view;

			cell.TextLabel.BackgroundColor = UIColor.Clear;
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.BackgroundColor = UIColor.Clear;
		}

		protected virtual void FixedCommon(UITableView tableView, StandardContentCell cell, NSIndexPath indexPath) {
			
			if (cell.TextLabel != null) {
				cell.TextLabel.Font = _controller._headingFont;
				cell.TextLabel.Text = Text;
			}

			if (BackgroundColor != null)
				ApplyBackgroundColor (cell, BackgroundColor);
			else if (_controller.CellBackgroundColor != null)
				ApplyBackgroundColor (cell, _controller.CellBackgroundColor);

			if (TextColor != null)
				cell.TextLabel.TextColor = TextColor;
			else if (_controller.CellTextColor != null)
				cell.TextLabel.TextColor = _controller.CellTextColor;

			if (TextFont != null)
				cell.TextLabel.Font = TextFont;

			if (DetailsFont != null && cell.DetailTextLabel != null)
				cell.DetailTextLabel.Font = DetailsFont;

			if (cell.DetailTextLabel != null) {
				if (DetailsColor != null)
					cell.DetailTextLabel.TextColor = DetailsColor;
				else if (_controller.CellDetailsColor != null)
					cell.DetailTextLabel.TextColor = _controller.CellDetailsColor;
			}

			if (Accessory != null)
				cell.Accessory = Accessory.Value;
			else 
				cell.Accessory = UITableViewCellAccessory.None;
			
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Details;
			
			if (LoadingIndicatorAccessory) {
				var loading = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
				//loading.Frame = new RectangleF(tableView.Frame.Width - (_controller._rightOffset * 2) - Height, 0, Height, Height);
				loading.StartAnimating ();
				cell.AccessoryView = loading;
			}
			
			if (StaticImage != null)
				SetImage (cell, UIImage.FromFile (StaticImage));
			else if (ImageGetter != null) 
				SetImage (cell, ImageGetter ());
			else 
				SetImage (cell, null);

			if (AfterGetCellInit != null) 
				AfterGetCellInit (cell);
		}
	}
	
	public class CustomContentRow : ContentRow {

		public bool CenterAlign;
		public bool Reuse = true;
		public string ReuseIdentifier;

		protected float GetImageSize() {
			if (StaticImage != null || ImageGetter != null || ImageUrl != null)
				return 44;
			else 
				return 0;
		}

		protected virtual float GetLeftOffsetPlusImage() {
			float imageSize = GetImageSize ();
			if (Math.Abs (imageSize - 0) > 0.1)
				imageSize += 8;

			return GetLeftOffset () + imageSize;
		}

		protected virtual float GetLeftOffset() {

			float imageOffset = 10;

			if (StaticImage != null || ImageGetter != null || ImageUrl != null)
				imageOffset = 0;

			if (_controller.IsPlainTable)
				return imageOffset;
			else 
				return (RichListController.IsIpad ? 54 : 10) + imageOffset;
		}

		protected virtual float GetRightOffset() {
			float offset = 0;
			if (!_controller.IsPlainTable)
				offset += (RichListController.IsIpad ? 54 : 20);

			if (Accessory.HasValue)
				offset += 20;

			if (_controller.IsPlainTable && this.CenterAlign)
				offset += 10;

			return offset;
		}
	}
	
	public abstract class ContentRow {
		public RichListController _controller;
		public string Text;
		public string Details;
		protected string Type { get { return GetType ().Name; } }		
		public Action Action;
		public Action ActionInEdit;
		public Action AccessoryAction;
		public int Height = 44;
		public int MinHeight;
		public int MaxHeight = 300;
		public string StaticImage;
		public string ImageUrl;
		public Func<UIViewController> NavController;
		public Func<UIViewController> ModalController;
		public UIModalPresentationStyle? ModalStyle;
		public UITableViewCellAccessory? Accessory;
		public bool LoadingIndicatorAccessory;
		public Action DeleteAction;
		public Func<UIImage> ImageGetter;
		public UIColor BackgroundColor;
		public bool IsFixed;
		public bool Disable;
		public string SearchDetailsField;
		public bool CropImageToSquare;

		public virtual UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
			return null;
		}

		protected virtual UITableViewCell BuildCell(UITableView tableView, NSIndexPath indexPath) {
			return null;
		}
		
		public virtual float GetHeight(UITableView tableView) {
			return Height;
		}

		public void AfterGetCell(UIScrollView tableView, UITableViewCell cell, NSIndexPath indexPath) {

			if (_controller.SmartImageDownload) {

				if (_controller.CollapseEnabled && indexPath.Row == 0)
					return;

				if (ImageUrl == null) {
					_controller._downloadedImages [indexPath] = true;
					SetNoImage (cell);
				} else {
					var cachedImage = _controller.ImageCacheGetter(ImageUrl);
					if (cachedImage == null) {
						
						SetNoImage (cell);
						
						if (!tableView.Dragging && !tableView.Decelerating) {
							_controller.RequestImage (ImageUrl, indexPath, _controller.ImageCallback);
						}
					} else {
						_controller._downloadedImages [indexPath] = true;
						SetImage (cell, cachedImage);
					}
				}
			}

			if (Disable) {
				cell.UserInteractionEnabled = false;
				cell.TextLabel.Enabled = false;

				if (cell.DetailTextLabel != null)
					cell.DetailTextLabel.Enabled = false;
			}
		}

		protected void SetNoImage(UITableViewCell cell) {
			
			var image = UIImage.FromFile (_controller.NoImagePath);
			SetImage (cell, image);
		}
		
		public virtual void SetImage(UITableViewCell cell, UIImage image) {
			if (cell.ImageView == null)
				return;

			if (CropImageToSquare && image != null) {

				var size = image.Size;

				if (Math.Abs (size.Width - size.Height) > 0.5f) {
					float width = Math.Min (size.Width, size.Height);
					image = new ImageResizer (image).CropResize (width, width, CropPosition.Center); 
				}
			}

			cell.ImageView.Image = image;
		}

		public virtual void RowSelected(UITableView tableView) {
			RowSelectedImpl(tableView);
		}

		public virtual void RowSelectedInEdit(UITableView tableView) {
			if (ActionInEdit != null)
				ActionInEdit ();
		}

		protected void RowSelectedImpl(UITableView tableView) {
			if (Action != null) {
				Action();
			}
			else if (NavController != null) {
				
				var newController = NavController();
				
				if (newController != null)
					RichListController.PushAction (_controller, newController);
			}
			else if (ModalController != null) {
				
				var newController = ModalController();
				var navController = new UINavigationController(newController);
				
				if (ModalStyle.HasValue)
					navController.ModalPresentationStyle = ModalStyle.Value;
				else if (RichListController.IsIpad)
					navController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
				
				_controller.PresentViewController (navController, true, null);
			}
			
			tableView.DeselectRow (tableView.IndexPathForSelectedRow, true);
		}
	}
	
	public class TextOnlyDynamicRow : CustomContentRow {

		public UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);
		public UIColor DetailsColor;
		public float PaddingLeft = 0;
		public float PaddingRight = 0;
		public Action<TextOnlyCell> AfterGetCellInit { get; set; }
		public Action<TextOnlyCell> AfterCreateCell { get; set; }

		public TextOnlyDynamicRow ()
		{
			ReuseIdentifier = "textonly";
		}

		public class TextOnlyCell : UITableViewCell {

			public UILabel _details;

			public TextOnlyCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
			{
				_details = new UILabel();
				_details.BackgroundColor = UIColor.Clear;
				_details.Lines = 0;
				Add (_details);
			}

			public void ReloadUI(TextOnlyDynamicRow row, UITableView tableView) {
				_details.Text = row.Text;
				_details.Font = row.DetailsFont;

				if (row.Action != null || row.NavController != null || row.ModalController != null)
					SelectionStyle = UITableViewCellSelectionStyle.Blue;
				else 
					SelectionStyle = UITableViewCellSelectionStyle.None;

				if (row.DetailsColor != null)
					_details.TextColor = row.DetailsColor;

				float leftOffsetPlusImage = row.GetLeftOffsetPlusImage ();
				float height = row.GetHeightCore (tableView);
				height = Math.Max (height, 24);
				_details.Frame = new RectangleF(leftOffsetPlusImage + row.PaddingLeft, 10, tableView.Frame.Width - row.GetRightOffset () - leftOffsetPlusImage - row.PaddingLeft - row.PaddingRight, height);

				if (row.Accessory.HasValue)
					Accessory = row.Accessory.Value;
				else 
					Accessory = UITableViewCellAccessory.None;

				if (row.BackgroundColor != null) 
					BackgroundView = new UIView { BackgroundColor = row.BackgroundColor };
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			TextOnlyCell cell = null;

			if (Reuse)
				cell = (TextOnlyCell)tableView.DequeueReusableCell (ReuseIdentifier);
				
			if (cell == null) {
				cell = new TextOnlyCell (ReuseIdentifier);

				if (AfterCreateCell != null)
					AfterCreateCell (cell);
			}
			
			cell.ReloadUI(this, tableView);

			if (AfterGetCellInit != null)
				AfterGetCellInit (cell);

			return cell;
		}

		public float GetHeightCore(UITableView tableView) {
			
			var size = tableView.StringSize (Text ?? "", DetailsFont, 
			                                 new SizeF(tableView.Frame.Width - GetRightOffset () - GetLeftOffsetPlusImage () - PaddingLeft - PaddingRight, MaxHeight));
			
			return size.Height;
		}

		public override float GetHeight (UITableView tableView)
		{
			if (Height > 0 && Height != 44)
				return Height;
		
			return Math.Max (GetHeightCore(tableView) + 18, 44);
		}
	}

	public class EmptyRow : CustomContentRow {

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("empty");

			if (cell == null) {
				cell = new UITableViewCell(UITableViewCellStyle.Default, "empty");
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;

				if (!_controller.IsPlainTable) {
					cell.BackgroundView = new UIView ();
				}
			}

			return cell;
		}

		public override float GetHeight (UITableView tableView)
		{
			return Height;
		}
	}

	public class SubtitleDynamicRow : CustomContentRow {

		public UIFont TextFont = UIFont.BoldSystemFontOfSize (17);
		public UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);
		public UIColor TextColor;
		public UIColor DetailsColor;
		public Action<SubtitleDynamicCell> AfterGetCellInit;

		public class SubtitleDynamicCell : UITableViewCell {

			public UILabel _text;
			public UILabel _details;
			public UIImageView _image;

			public SubtitleDynamicCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
			{
				_text = new UILabel();
				_text.BackgroundColor = UIColor.Clear;
				Add (_text);

				_details = new UILabel();
				_details.BackgroundColor = UIColor.Clear;
				_details.TextColor = UIColor.Gray;
				_details.Lines = 0;
				Add (_details);

				_image = new UIImageView();
				_image.ContentMode = UIViewContentMode.ScaleAspectFit;
				Add (_image);
			}

			public void ReloadUI(SubtitleDynamicRow row, UITableView tableView) {
				_text.Text = row.Text ?? "";
				_details.Text = row.Details ?? "";

				_text.Font = row.TextFont;
				_details.Font = row.DetailsFont;

				if (row.DetailsColor != null)
					_details.TextColor = row.DetailsColor;

				if (row.TextColor != null)
					_text.TextColor = row.TextColor;

				if (row.Accessory.HasValue) {
					Accessory = row.Accessory.Value;
				}

				float leftOffset = row.GetLeftOffset ();
				float rightOffset = row.GetRightOffset ();
				float imageSize = row.GetImageSize ();
				float leftOffsetAndImage = row.GetLeftOffsetPlusImage ();

				if (row.ImageUrl != null) {
					_image.Frame = new RectangleF (leftOffset, 2, imageSize, imageSize);
					//leftOffset += imageSize + 8;
				}
				else if (row.StaticImage != null) {
					row.SetImage (this, UIImage.FromFile (row.StaticImage));
					_image.Frame = new RectangleF(leftOffset, 2, imageSize, imageSize);
					//leftOffset += imageSize + 8;
				}
				else if (row.ImageGetter != null) {
					row.SetImage (this, row.ImageGetter());
					_image.Frame = new RectangleF(leftOffset, 2, imageSize, imageSize);
					//leftOffset += imageSize + 8;
				} 
				else {
					_image.Frame = RectangleF.Empty;
				}

				_text.Frame = new RectangleF(leftOffsetAndImage, 0, tableView.Frame.Width - rightOffset - leftOffsetAndImage, 30);
				_details.Frame = new RectangleF(leftOffsetAndImage, 30, tableView.Frame.Width - rightOffset - leftOffsetAndImage, row.GetHeightCore(tableView));

				if (row.Disable) {
					UserInteractionEnabled = false;
					_text.Enabled = false;
					_details.Enabled = false;
				}

				if (row.CenterAlign) {
					_text.TextAlignment = UITextAlignment.Center;
					_details.TextAlignment = UITextAlignment.Center;
				}

				if (row.BackgroundColor != null)
					BackgroundView = new UIView { BackgroundColor = row.BackgroundColor };
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (SubtitleDynamicCell)tableView.DequeueReusableCell ("subtitledynamic");

			if (cell == null)
				cell = new SubtitleDynamicCell("subtitledynamic");

			cell.ReloadUI(this, tableView);

			if (AfterGetCellInit != null)
				AfterGetCellInit (cell);

			return cell;
		}

		private float GetHeightCore(UITableView tableView) {

			var size = tableView.StringSize (Details ?? "", DetailsFont, 
			                                 new SizeF(tableView.Frame.Width - GetLeftOffsetPlusImage () - GetRightOffset (), MaxHeight));
			
			return size.Height;
		}

		public override float GetHeight (UITableView tableView)
		{
			return Math.Max (GetHeightCore(tableView) + 36, 44);
		}

		public override void SetImage (UITableViewCell cell, UIImage image)
		{
			((SubtitleDynamicCell)cell)._image.Image = image;
		}
	}

	public class PaddedRow : CustomContentRow {

		public UIFont TextFont = UIFont.BoldSystemFontOfSize (17);
		public UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);
		public UIColor TextColor;
		public UIColor DetailsColor;
		public Action<SubtitleDynamicCell> AfterGetCellInit;
		public UIColor PaddedColor;
		public float Padding = 16;

		public class SubtitleDynamicCell : UITableViewCell {

			public UILabel _text;
			public UILabel _details;
			public UIImageView _image;

			public SubtitleDynamicCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
			{
				_text = new UILabel();
				_text.BackgroundColor = UIColor.Clear;
				Add (_text);

				_details = new UILabel();
				_details.BackgroundColor = UIColor.Clear;
				_details.TextColor = UIColor.Gray;
				_details.Lines = 0;
				Add (_details);

				_image = new UIImageView();
				_image.ContentMode = UIViewContentMode.ScaleAspectFit;
				Add (_image);
			}

			public void ReloadUI(PaddedRow row, UITableView tableView) {
				_text.Text = row.Text ?? "";
				_details.Text = row.Details ?? "";

				_text.Font = row.TextFont;
				_details.Font = row.DetailsFont;

				if (row.DetailsColor != null)
					_details.TextColor = row.DetailsColor;

				if (row.TextColor != null)
					_text.TextColor = row.TextColor;

				if (row.Accessory.HasValue) {
					Accessory = row.Accessory.Value;
				}

				float leftOffset = row.GetLeftOffset ();
				float rightOffset = row.GetRightOffset ();
				float imageSize = row.GetImageSize ();
				float leftOffsetAndImage = row.GetLeftOffsetPlusImage ();
				float heightCore = row.GetHeightCore (tableView);
			
				if (row.ImageUrl != null) {
					_image.Frame = new RectangleF (leftOffset, 8, imageSize, imageSize);
					//leftOffset += imageSize + 8;
				}
				else if (row.StaticImage != null) {
					row.SetImage (this, UIImage.FromFile (row.StaticImage));
					_image.Frame = new RectangleF(leftOffset, 8, imageSize, imageSize);
					//leftOffset += imageSize + 8;
				}
				else if (row.ImageGetter != null) {
					row.SetImage (this, row.ImageGetter());
					_image.Frame = new RectangleF(leftOffset, 8, imageSize, imageSize);
					//leftOffset += imageSize + 8;
				} 
				else {
					_image.Frame = RectangleF.Empty;
				}

				_text.Frame = new RectangleF(leftOffsetAndImage, 0, tableView.Frame.Width - rightOffset - leftOffsetAndImage, 30);
				_details.Frame = new RectangleF(leftOffsetAndImage, 30, tableView.Frame.Width - rightOffset - leftOffsetAndImage, heightCore);

				if (row.Disable) {
					UserInteractionEnabled = false;
					_text.Enabled = false;
					_details.Enabled = false;
				}

				if (row.CenterAlign) {
					_text.TextAlignment = UITextAlignment.Center;
					_details.TextAlignment = UITextAlignment.Center;
				}

				if (row.BackgroundColor != null)
					BackgroundView = new UIView { BackgroundColor = row.BackgroundColor };

				var view = new UIView();
				view.BackgroundColor = row.PaddedColor;
				view.Layer.BorderColor = UIColor.Black.CGColor;
				view.Layer.BorderWidth = 1f;

				ContentView.Add(view);
				BringSubviewToFront(_text);
				BringSubviewToFront(_details);
				BringSubviewToFront(_image);

				view.Frame = new RectangleF(row.Padding, 4, tableView.Frame.Width - row.Padding - row.Padding, row.GetHeight(tableView) - 8);
			}
		}

		protected override float GetLeftOffset ()
		{
			return base.GetLeftOffset () + Padding + 4;
		}

		protected override float GetLeftOffsetPlusImage ()
		{
			return base.GetLeftOffsetPlusImage () + 4;
		}

		protected override float GetRightOffset ()
		{
			return base.GetRightOffset () + Padding + 4;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (SubtitleDynamicCell)tableView.DequeueReusableCell ("subtitledynamic");

			if (cell == null)
				cell = new SubtitleDynamicCell("subtitledynamic");

			cell.ReloadUI(this, tableView);

			if (AfterGetCellInit != null)
				AfterGetCellInit (cell);

			return cell;
		}

		private float GetHeightCore(UITableView tableView) {

			var size = tableView.StringSize (Details ?? "", DetailsFont, 
			                                 new SizeF(tableView.Frame.Width - GetLeftOffsetPlusImage () - GetRightOffset (), MaxHeight));

			return size.Height;
		}

		public override float GetHeight (UITableView tableView)
		{
			return Math.Max (GetHeightCore(tableView) + 36, 44);
		}

		public override void SetImage (UITableViewCell cell, UIImage image)
		{
			((SubtitleDynamicCell)cell)._image.Image = image;
		}
	}

	public class TitleDynamicRow : CustomContentRow {

		public UIFont TextFont = UIFont.BoldSystemFontOfSize(17);
		public UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);
		public UIColor TextColor;
		public UIColor DetailsColor = UIColor.Gray;
		public Action<TitleDynamicCell> AfterGetCellInit;

        public class TitleDynamicCell : UITableViewCell {

    		private UILabel _text;
    		private UILabel _details;
    		public UIImageView _image;

            public TitleDynamicCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier) {
                _text = new UILabel();
                _text.Lines = 0;
                _text.BackgroundColor = UIColor.Clear;
                Add (_text);

                _details = new UILabel();
                _details.BackgroundColor = UIColor.Clear;
                Add (_details);

                _image = new UIImageView();
                _image.ContentMode = UIViewContentMode.ScaleAspectFit;
                Add (_image);
            }

            public void ReloadUI(TitleDynamicRow row, UITableView tableView) {
                _text.Text = row.Text;
                _details.Text = row.Details;

				_text.Font = row.TextFont;
				_details.Font = row.DetailsFont;

				if (row.TextColor != null)
					_text.TextColor = row.TextColor;

				_details.TextColor = row.DetailsColor;

                if (row.Accessory.HasValue) {
                    Accessory = row.Accessory.Value;
                }

				float leftOffset = row.GetLeftOffset ();
                float rightOffset = row.GetRightOffset();
				float leftOffsetWithImage = row.GetLeftOffsetPlusImage ();

                if (row.StaticImage != null) {
					row.SetImage (this, UIImage.FromFile (row.StaticImage));
                    _image.Frame = new RectangleF(leftOffset, 2, 40, 40);
                }
                else if (row.ImageGetter != null) {
					row.SetImage (this, row.ImageGetter ());
                    _image.Frame = new RectangleF(leftOffset, 2, 40, 40);
                }
                else if (row.ImageUrl != null) {
                }
                else 
                    _image.Frame = RectangleF.Empty;

                float textHeight = row.GetHeightCore (tableView);
				_text.Frame = new RectangleF(leftOffsetWithImage, 0, tableView.Frame.Width - rightOffset - leftOffsetWithImage, textHeight + 10);
				_details.Frame = new RectangleF(leftOffsetWithImage, textHeight + 10, tableView.Frame.Width - rightOffset - leftOffsetWithImage, 20);

                if (row.BackgroundColor != null)
					BackgroundView = new UIView { BackgroundColor = row.BackgroundColor };
            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TitleDynamicCell)tableView.DequeueReusableCell ("titledynamic");

            if (cell == null)
                cell = new TitleDynamicCell("titledynamic");

            cell.ReloadUI(this, tableView);

			if (AfterGetCellInit != null)
				AfterGetCellInit (cell);

            return cell;
        }

		public float GetHeightCore(UITableView tableView) {
			var size = tableView.StringSize (Text ?? "", TextFont, 
			                                 new SizeF(tableView.Frame.Width - GetRightOffset () - GetLeftOffsetPlusImage (), MaxHeight));
			
			return size.Height;
		}
		
		public override float GetHeight (UITableView tableView)
		{
			return Math.Max (GetHeightCore(tableView) + 30, 44);
		}

		public override void SetImage (UITableViewCell cell, UIImage image)
		{
			((TitleDynamicCell)cell)._image.Image = image;
		}
	}
	
	public class Value1DynamicRow : CustomContentRow {

		public UIFont TextFont = UIFont.BoldSystemFontOfSize (17);
		public UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 17);
		public UIColor DetailsColor = RichListController.FromHex("385487");

		private class InnerCell : UITableViewCell {

			private UILabel _text;
			private UILabel _details;

			public InnerCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
			{
				_text = new UILabel();
				_text.Lines = 0;
				_text.BackgroundColor = UIColor.Clear;

				_details = new UILabel();
				_details.TextAlignment = UITextAlignment.Right;
				_details.BackgroundColor = UIColor.Clear;

				Add (_text);
				Add (_details);
			}

			public void ReloadUI(Value1DynamicRow row, UITableView tableView) {
				_text.Text = row.Text ?? "";
				_details.Text = row.Details ?? "";

				_text.Font = row.TextFont;
				_details.Font = row.DetailsFont;
				_details.TextColor = row.DetailsColor;

				float height = row.GetHeightCore(tableView);
				_text.Frame = new RectangleF(row.GetOffset (), 4, tableView.Frame.Width - (row.GetOffset () * 2) - 32, height);
				_details.Frame = new RectangleF(tableView.Frame.Width - (row.GetOffset () * 2) - 24, 4, row.GetOffset () + 24, height);
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (InnerCell)tableView.DequeueReusableCell ("value1dynamic");

			if (cell == null)
				cell = new InnerCell("value1dynamic");

			cell.ReloadUI(this, tableView);

			return cell;
		}
		
		private float GetHeightCore(UITableView tableView) {
			
			//if (Accessory.HasValue) {
			//	float rightOffset = (_controller._rightOffset * 2);
			//	rightOffset += _controller._rightOffset;
			//}
			
			var size = tableView.StringSize (Text ?? "", _controller._headingFont, 
			                                 new SizeF(tableView.Frame.Width - (GetOffset () * 2) - 24, 80));
			
			return Math.Max (34, size.Height);
		}

		public float GetOffset() {
			if (_controller.IsPlainTable)
				return 0;
			else 
				return RichListController.IsIpad ? 54 : 20;
		}

		public override float GetHeight (UITableView tableView)
		{
			return Math.Max (GetHeightCore(tableView) + 10, 44);
		}
	}

	public class TDBadgedCell : UITableViewCell
	{
		public BadgeSettings Settings { get; set; }
		private TDBadgeView Badge {get;set;}
		private TDBadgeView Badge2 {get;set;}
		
		public TDBadgedCell (UITableViewCellStyle style, string reuseIdentifier) : base (style, reuseIdentifier)
		{
			Badge = new TDBadgeView(RectangleF.Empty);
			Badge.Parent = this;
			
			this.ContentView.AddSubview(this.Badge);
		} 
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews();
			
			if (Settings.BadgeNumber2 != null && Badge2 == null) {
				Badge2 = new TDBadgeView(RectangleF.Empty);
				Badge2.Parent = this;
				
				ContentView.AddSubview(Badge2);
			}
			
			LayoutSubviewsImpl(Settings.BadgeNumber, Badge, Settings.BadgeColor, Settings.BadgeColorHighlighted);
			if (Settings.BadgeNumber2 != null && Badge2 != null) 
				LayoutSubviewsImpl(Settings.BadgeNumber2, Badge2, Settings.BadgeColor2, Settings.BadgeColorHighlighted2, isSecondBadge:true);
		}
		
		private void LayoutSubviewsImpl(string badgeNum, TDBadgeView badge, UIColor badgeColor, UIColor badgeColorHighlighted, bool isSecondBadge = false) {
			if (!string.IsNullOrEmpty(badgeNum))
			{
				//force badges to hide on edit.
				if (Editing)
					badge.Hidden = true;
				else
					badge.Hidden = false;
				
				var ns = new NSString(badgeNum);
				SizeF badgeSize = ns.StringSize(UIFont.BoldSystemFontOfSize(14));
				
				RectangleF badgeFrame;
				
				float additionalOffset = 0;
				if (isSecondBadge) {
					float firstBadgeWidth = new NSString(Settings.BadgeNumber).StringSize (UIFont.BoldSystemFontOfSize(14)).Width;
					additionalOffset = firstBadgeWidth + 16 + 4;
				}
				
				badgeFrame = new RectangleF(ContentView.Frame.Size.Width - (badgeSize.Width+16) - 10 - additionalOffset
				                            , Convert.ToSingle(Math.Round((ContentView.Frame.Size.Height - 18) /2))
				                            , badgeSize.Width + 16f
				                            , 18f);
				
				badge.Frame = badgeFrame;
				badge.BadgeNumber = badgeNum;
				badge.Parent = this;
				
				if (TextLabel.Frame.X + TextLabel.Frame.Size.Width >= badgeFrame.X)
				{
					float badgeWidth = Convert.ToSingle(TextLabel.Frame.Size.Width - badgeFrame.Size.Width - 10.0);
					
					TextLabel.Frame = new RectangleF(TextLabel.Frame.X
					                                      , TextLabel.Frame.Y
					                                      , badgeWidth
					                                      , TextLabel.Frame.Size.Height);
				}
				
				if ((DetailTextLabel.Frame.X + DetailTextLabel.Frame.Size.Width) >= badgeFrame.X)
				{
					float badgeWidth = Convert.ToSingle(DetailTextLabel.Frame.Size.Width - badgeFrame.Size.Width - 10);
					DetailTextLabel.Frame = new RectangleF(DetailTextLabel.Frame.X
					                                       , DetailTextLabel.Frame.Y
					                                       , badgeWidth
					                                       , DetailTextLabel.Frame.Size.Height);
				}
				
				//set badge hightlighed colours or use defaults
				if (badgeColorHighlighted != null)
					badge.BadgeColorHighlighted = badgeColorHighlighted;
				else
					badge.BadgeColorHighlighted = UIColor.FromRGBA(1.0f, 1.0f, 1.0f, 1.000f);
				
				if (badgeColor != null)
					badge.BadgeColor = badgeColor;
				else
					badge.BadgeColor = UIColor.FromRGBA(0.530f, 0.600f, 0.738f, 1.000f);
			}
			else
			{
				badge.Hidden = true;
			}
		}
		
		public override void SetHighlighted (bool highlighted, bool animated)
		{
			base.SetHighlighted (highlighted, animated);
			Badge.SetNeedsDisplay();
			if (Badge2 != null)
				Badge2.SetNeedsDisplay();
		}
		public override void SetSelected (bool selected, bool animated)
		{
			base.SetSelected (selected, animated);
			Badge.SetNeedsDisplay();
			if (Badge2 != null)
				Badge2.SetNeedsDisplay();
		}
		public override void SetEditing (bool editing, bool animated)
		{
			base.SetEditing (editing, animated);
			if (editing)
			{
				Badge.Hidden = true;
				Badge.SetNeedsDisplay();
				
				if (Badge2 != null) {
					Badge2.Hidden = true;
					Badge2.SetNeedsDisplay();
				}
				
				SetNeedsDisplay();
			}
			else
			{
				Badge.Hidden = false;
				Badge.SetNeedsDisplay();
				
				if (Badge2 != null) {
					Badge2.Hidden = false;
					Badge2.SetNeedsDisplay();
				}
				
				SetNeedsDisplay();
			}
		}
	}

	public class TDBadgeView : UIView
	{
		public int Width {get;set;}
		public string BadgeNumber {get;set;}
		
		SizeF numberSize;
		UIFont font;
		string countString;
		public UITableViewCell Parent {get;set;}
		
		public UIColor BadgeColor {get;set;}
		public UIColor BadgeColorHighlighted {get;set;}
		
		public TDBadgeView (RectangleF frame) : base (frame)
		{
			font = UIFont.BoldSystemFontOfSize(14f);
			this.BackgroundColor = UIColor.Clear;
		}
		public override void Draw (RectangleF rect)
		{
			countString = BadgeNumber;
			var ns = new NSString(countString);
			numberSize = ns.StringSize (font);
			
			Width = Convert.ToInt32(numberSize.Width + 16);
			
			var bounds = new RectangleF(0, 0, numberSize.Width + 16, 18);
			
			var context = UIGraphics.GetCurrentContext();
			
			float radius = bounds.Size.Height / 2.0f;
			
			context.SaveState();
			
			if (Parent.Highlighted || Parent.Selected)
			{
				UIColor col;
				
				if (BadgeColorHighlighted != null)
					col = BadgeColorHighlighted;
				else
					col = UIColor.FromRGBA (1.0f, 1.0f, 1.0f, 1.000f);
				
				context.SetFillColorWithColor (col.CGColor);
			}
			else
			{
				UIColor col;
				if (BadgeColor != null)
					col = BadgeColor;
				else 
					col = UIColor.FromRGBA(0.530f, 0.600f, 0.738f, 1.000f);
				
				context.SetFillColorWithColor (col.CGColor);
			}
			
			context.BeginPath();
			float a = Convert.ToSingle(Math.PI / 2f);
			float b = Convert.ToSingle(3f * Math.PI / 2f);
			context.AddArc(radius, radius, radius, a, b, false);
			context.AddArc(bounds.Size.Width - radius, radius, radius, b, a, false);
			context.ClosePath();
			context.FillPath();
			context.RestoreState();
			
			bounds.X = (bounds.Size.Width - numberSize.Width) / 2 + 0.5f;
			
			context.SetBlendMode(CGBlendMode.Clear);
			
			DrawString(countString, bounds, font);
		}
	}

	public class SpotNewsDynamicRow : CustomContentRow {
		
		public string Byline;
		public string BadgeText;
		public float MaxDetailsHeight;
		public float MaxTitleHeight;
		public UIFont HeadingFont = UIFont.BoldSystemFontOfSize(17);
		public UIFont DetailsFont = UIFont.FromName("HelveticaNeue", 14);

		public bool HasImage { get { return ImageUrl != null || ImageGetter != null ; } }
		
		private class SpotNewsCell : UITableViewCell {
			private UILabel _text;
			private UILabel _details;
			private UILabel _byline;
			public UIImageView _image;
			private TDBadgeView _badge;
			
			public SpotNewsCell (string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
			{
				_text = new UILabel();
				_text.BackgroundColor = UIColor.Clear;
				_text.Lines = 0;
				Add (_text);
				
				_details = new UILabel();
				_details.BackgroundColor = UIColor.Clear;
				_details.TextColor = UIColor.Gray;
				_details.Lines = 0;
				Add (_details);
				
				_byline = new UILabel();
				_byline.Font = UIFont.ItalicSystemFontOfSize(12);
				_byline.BackgroundColor = UIColor.Clear;
				_byline.TextColor = UIColor.Gray;
				Add (_byline);
				
				_image = new UIImageView();
				_image.ContentMode = UIViewContentMode.ScaleAspectFit;
				Add (_image);
				
				_badge = new TDBadgeView(RectangleF.Empty);
				_badge.Parent = this;
				Add (_badge);	
			}
			
			public void ReloadUI(SpotNewsDynamicRow row, UITableView tableView) {
				
				_text.Text = row.Text;
				_byline.Text = row.Byline;

				_text.Font = row.HeadingFont;
				_details.Font = row.DetailsFont;

				if (!string.IsNullOrEmpty(row.Details))
					_details.Text = row.Details;

				float titleHeight = row.GetHeightTitle(tableView);
				float detailsHeight = row.GetHeightDetails(tableView);
				
				float leftOffset = row.HasImage ? 58 : 18;
				
				_text.Frame = new RectangleF(leftOffset, 0, tableView.Frame.Width - leftOffset - 18, titleHeight);
				_details.Frame = new RectangleF(leftOffset, titleHeight, tableView.Frame.Width - leftOffset - 18, detailsHeight);
				_byline.Frame = new RectangleF(leftOffset, detailsHeight + titleHeight, tableView.Frame.Width - leftOffset - 18, 20);
				
				if (row.HasImage)
					_image.Frame = new RectangleF(4, 4, 50, row.GetHeight (tableView) - 8);

				if (row.ImageGetter != null)
					row.SetImage (this, row.ImageGetter());

				if (!string.IsNullOrEmpty (row.BadgeText)) {
					_badge.Frame = new RectangleF(tableView.Frame.Width - 40, detailsHeight + titleHeight, 30, 20);
					_badge.BadgeNumber = row.BadgeText;
					DecreaseWidth(_byline, 40);
					_badge.SetNeedsDisplay ();
				}
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
			var cell = (SpotNewsCell)tableView.DequeueReusableCell ("spotnews");
			
			if (cell == null)
				cell = new SpotNewsCell("spotnews");
			
			cell.ReloadUI(this, tableView);
			
			return cell;
		}
		
		public override void SetImage (UITableViewCell cell, UIImage image)
		{
			((SpotNewsCell)cell)._image.Image = image;
		}
		
		public float GetHeightDetails(UITableView tableView) {

			if (string.IsNullOrEmpty (Details))
				return 0;

			float leftOffset = HasImage ? 58 : 18;
			
			var size = tableView.StringSize (Details, DetailsFont, 
			                                 new SizeF(tableView.Frame.Width - leftOffset - 18, MaxDetailsHeight));
			
			return size.Height;
		}
		
		public float GetHeightTitle(UITableView tableView) {
			
			float leftOffset = HasImage ? 58 : 18;
			
			var size2 = tableView.StringSize (Text ?? "", HeadingFont, 
			                                  new SizeF(tableView.Frame.Width - leftOffset - 18, MaxTitleHeight));
			
			return size2.Height;
		}
		
		public override float GetHeight (UITableView tableView)
		{
			return Math.Max (GetHeightTitle(tableView) + GetHeightDetails (tableView) + 26, 44);
		}
	}
}

