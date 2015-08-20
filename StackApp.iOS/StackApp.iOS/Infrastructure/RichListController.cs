using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Net;
using System.Globalization;

namespace Abilitics.iOS
{
	public class RichListController : UIViewController
	{
		public static bool IsIpad {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad; }
		}

		public static UIColor FromHex (string hex)
		{
			byte red;
			byte green;
			byte blue;

			HexToRgb (hex, out red, out green, out blue);
			var color = UIColor.FromRGB (red, green, blue);

			return color;
		}

		private static void HexToRgb (string hex, out byte red, out byte green, out byte blue)
		{
			hex = hex.TrimStart ('#');

			if (hex.Length == 6) {
				red = byte.Parse (hex.Substring (0, 2), NumberStyles.AllowHexSpecifier);
				green = byte.Parse (hex.Substring (2, 2), NumberStyles.AllowHexSpecifier);
				blue = byte.Parse (hex.Substring (4, 2), NumberStyles.AllowHexSpecifier);
			} else if (hex.Length == 3) {
				red = byte.Parse (hex[0].ToString () + hex[0], NumberStyles.AllowHexSpecifier);
				green = byte.Parse (hex[1].ToString () + hex[1], NumberStyles.AllowHexSpecifier);
				blue = byte.Parse (hex[2].ToString () + hex[2], NumberStyles.AllowHexSpecifier);
			} else {
				throw new ArgumentException ("Hex string is invalid: " + hex);
			}
		}

		public Dictionary<NSIndexPath, bool> _downloadedImages;
		public List<CollapseRow> _collapseRows;
		public List<CollapseRow> _originalCollapseRows;

		public readonly UIFont _headingFont = UIFont.BoldSystemFontOfSize(17);
		public readonly UIFont _subtitleDetailsFont = UIFont.FromName("HelveticaNeue", 14);
		public readonly float _rightOffset = IsIpad ? 54 : 20;

		public List<Tuple<string, List<ContentRow>>> _items;
		public List<Tuple<string, List<ContentRow>>> _originalItems;
		private int currentHeaderIndex = -1;
		protected bool _isReloading;
		public bool CollapseEnabled;
		public int CollapseIndentLevel = 2;
		public int PagingSectionIndex = 0;

		public static Action<UIViewController, UIViewController> PushAction = (parent, child) =>
		{
			parent.NavigationController.PushViewController (child, true);
		};

		public UIColor SectionHeaderColor;
		protected float HeaderHeight = 22; // this is the default value

		protected UIToolbar _bottomToolbar;

		protected UITableView _tableView;

		protected bool EnableSearchBar { get; set; }
		protected bool EnableSearchBarInNavBar { get; set; }
		protected bool LongRunning { get; set; }
		protected UITableViewCellAccessory? DefaultAccessory { get; set; }
		protected bool EnableEditing { get; set; }
		public bool IsPlainTable { get; set; }
		protected bool UseIndex { get; set; }
		public bool SmartImageDownload { get; set; }
		protected bool EnablePaging { get; set; }
		protected Action PagingAction { get; set; }
        protected Action<Action<int>> InfinitePagingAction { get; set; }
		protected bool HasMorePages { get; set; }
        protected bool EnableInfinitePaging { get; set; }
        protected bool _pagingActionInProgress;
		protected string NoResultsMessage { get; set; }
		public string NoImagePath { get; set; }
		protected UITableViewCellSeparatorStyle SeparatorStyle { get; set; }
		public Func<string, UIImage> ImageCacheGetter { get; set; } 
		protected Action<HttpWebRequest> ImageWebRequestInit { get; set; }
		public UIColor CellBackgroundColor { get; set; }
		public UIColor CellTextColor { get; set; }
		public UIColor CellDetailsColor { get; set; }
		public Func<int, float> GetHeightForHeader { get; set; }

		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);
			_tableView.ReloadData ();
		}

		public RichListController ()
		{
			_items = new List<Tuple<string, List<ContentRow>>>();

			NoImagePath = "noimage.png";
			SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
		}
				
		protected void RemoveSection() {
			_items.RemoveAt (0);
			_tableView.BeginUpdates ();
			_tableView.DeleteSections (NSIndexSet.FromIndex (0), UITableViewRowAnimation.Fade);
			_tableView.EndUpdates ();
		}
		
		protected void StartingReloadAllData() {
			_isReloading = true;
			
			InvokeOnMainThread (delegate {
				if (_tableView != null) 
					_tableView.ScrollEnabled = false;
			});
		}
		
		protected void FinishedReloadAllData() {
			_isReloading = false;
			
			InvokeOnMainThread (delegate {
				if (_tableView != null) 
					_tableView.ScrollEnabled = true;
			});
		}
		
		protected void AddBottomToolbar(params UIBarButtonItem[] items) {
			_bottomToolbar = new UIToolbar ();
			_bottomToolbar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleTopMargin;
			
			if (items.Any ())
				_bottomToolbar.SetItems (items, false);
			
			Add (_bottomToolbar);
		}
		
		protected void ClearAllRows() {
			_items = new List<Tuple<string, List<ContentRow>>>();
			InitOriginalItems ();
			currentHeaderIndex = -1;

			if (_collapseRows != null)
				_collapseRows.Clear ();
		}

		private void InitOriginalItems() {
			_originalItems = new List<Tuple<string, List<ContentRow>>>();

			foreach (var i in _items)
				_originalItems.Add (Tuple.Create (i.Item1, i.Item2.ToList ()));
		}

		private void InitOriginalCollapseRows() {

			if (_collapseRows == null)
				return;

			_originalCollapseRows = new List<CollapseRow> (_collapseRows);
		}

		public override void ViewDidLoad ()
		{
			if (_tableView == null)
				CreateTableView();
			
			if (LongRunning)
				_tableView.Source = new LoadingDataSource();
			else
				InitializeUI(forceRefresh: false);
		}

		private void CreateTableView() {
			_tableView = new UITableView(RectangleF.Empty, IsPlainTable ? UITableViewStyle.Plain : UITableViewStyle.Grouped);
			_tableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_tableView.SeparatorStyle = SeparatorStyle;
			Add(_tableView);
		}

		protected void ForceReloadData() {
			InitializeUI (forceRefresh: true);
		}

		private void InitializeUI(bool forceRefresh) {

			if (_tableView == null)
				return;

			// remove empty sections
			_items = _items.Where (i => i.Item2.Any ()).ToList ();

			if (!_items.Any ()) {
				_tableView.Source = new NoResultsDataSource(NoResultsMessage);
				_tableView.ReloadData ();
				return;
			}

			if (EnableSearchBar) {
				InitializeStickySearchBar ();
				InitOriginalItems ();
			} else if (EnableSearchBarInNavBar) {
				InitializeSearchBarInNavBar ();
				InitOriginalItems ();
			} else if (CollapseEnabled) {
				InitOriginalItems ();
			}

			InitOriginalCollapseRows ();

			if (_tableView != null) {
				_tableView.Source = new RichTableSource(this);
				if (LongRunning || forceRefresh)
					_tableView.ReloadData ();
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			if (_bottomToolbar != null) {
				if (_tableView != null)
					_tableView.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height - 44);

				_bottomToolbar.Frame = new RectangleF(0, View.Frame.Height - 44, View.Frame.Width, 44);
			} else {
				if (_tableView != null)
					_tableView.Frame = View.Frame;
			}

			_keyboardDidShowObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, KeyboardDidShow);
			_keyboardDidHideObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidHideNotification, KeyboardDidHide);
		}

		private NSObject _keyboardDidShowObserver;
		private NSObject _keyboardDidHideObserver;

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			if (_navSearchBar != null && !_navSearchBar.Hidden && NavigationController.NavigationBarHidden)
				SearchBarCancelClicked ();

			if (_keyboardDidHideObserver != null)
				NSNotificationCenter.DefaultCenter.RemoveObserver (_keyboardDidHideObserver);

			if (_keyboardDidShowObserver != null)
				NSNotificationCenter.DefaultCenter.RemoveObserver (_keyboardDidShowObserver);				
		}

		private void KeyboardDidShow(NSNotification notif) {
			NSObject value;
			notif.UserInfo.TryGetValue (UIKeyboard.FrameBeginUserInfoKey, out value);
			var rect = ((NSValue)value).RectangleFValue;
			var insets = new UIEdgeInsets(0, 0, rect.Height, 0);
			_tableView.ContentInset = insets;
			_tableView.ScrollIndicatorInsets = insets;
		}

		private void KeyboardDidHide(NSNotification notif) {
			UIView.Animate(0.3d, delegate {
				_tableView.ContentInset = UIEdgeInsets.Zero;
				_tableView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
			});
		}
		
		protected void DeselectSelectedRow() {
			_tableView.DeselectRow (_tableView.IndexPathForSelectedRow, true);
		}

		public void CollapseAll() {

			if (!CollapseEnabled)
				return;

			foreach (var item in _items) {
				item.Item2.Clear ();
			}

			_tableView.ReloadData ();
		}

		public void ExpandAll() {

			if (!CollapseEnabled)
				return;

			_items = _originalItems;
			InitOriginalItems ();

			_tableView.ReloadData ();
		}
		
		protected void AddTableHeader(UIView view) {
			_tableView.TableHeaderView = view;
		}

		private static bool IsBasicLatinLetter(char c) {
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
		}

		protected void AddRows(List<ContentRow> items) {
			
			if (UseIndex) {
				foreach(var group in items.Where (i => !string.IsNullOrEmpty (i.Text) && IsBasicLatinLetter(i.Text[0])).GroupBy (p => p.Text[0].ToString ().ToUpperInvariant()).OrderBy (p => p.Key)) {
					
					foreach(var i in group) 
						BeforeAddingRow (i);
					
					_items.Add (new Tuple<string, List<ContentRow>>(group.Key, group.ToList ()));
				}

				var empty = items.Where (i => string.IsNullOrEmpty (i.Text) || !IsBasicLatinLetter (i.Text[0])).ToList ();

				foreach (var i in empty)
					BeforeAddingRow (i);

				_items.Add (new Tuple<string, List<ContentRow>>("*", empty.ToList ()));

			} else {
				foreach (var item in items)
					AddRow (item);
			}
		}
		
		protected void AddRow(ContentRow item) {

			if (UseIndex)
				throw new InvalidOperationException ("Cannot use AddRow when UseIndex is true, use AddRows instead");

			if (currentHeaderIndex == -1) {
				if (CollapseEnabled)
					AddCollapseRow ("", 0);
				else 
					AddHeaderRow ("");

				currentHeaderIndex = 0;
			}

			BeforeAddingRow (item);

			if (_items.Count == currentHeaderIndex) 
				currentHeaderIndex--;

			var container = _items[currentHeaderIndex];
			container.Item2.Add(item);
		}

		protected void AddValue1FixedRow(string text, string details = null, string image = null) {
			AddRow(new Value1FixedRow  { 
				Text = text, 
				Details = details,
				StaticImage = image
			});
		}

		protected void AddEmptyRow(int height) {
			var row = new EmptyRow { 
				Height = height
			};

			AddRow (row);
		}

		protected void AddTextOnlyDynamicRow(string text, UIFont font = null) {
			var row = new TextOnlyDynamicRow { 
				Text = text, 
				MaxHeight = 10000
			};

			if (font != null)
				row.DetailsFont = font;

			AddRow (row);
		}

		protected void AddDefaultFixedRow(string text) {
			AddRow(new DefaultFixedRow  { 
				Text = text
			});
		}

		protected void AddSubtitleFixedRow(string text, string details = null) {
			AddRow(new SubtitleFixedRow  { 
				Text = text, 
				Details = details
			});
		}

		protected void AddSubtitleDynamicRow(string text, string details = null, string image = null) {
			AddRow(new SubtitleDynamicRow  { 
				Text = text, 
				Details = details,
				StaticImage = image
			});
		}

		private void BeforeAddingRow(ContentRow item) {

			item._controller = this;

			if (!item.Accessory.HasValue)
				item.Accessory = DefaultAccessory;
		}
		
		protected void AddHeaderRow(string header) {

			var lastItem = _items.LastOrDefault ();
			if (lastItem != null && !lastItem.Item2.Any ())
				_items.Remove (lastItem);

			currentHeaderIndex++;
			_items.Add(new Tuple<string, List<ContentRow>>(header, new List<ContentRow>()));
		}

		protected void AddCollapseRow(string header, int count) {
			AddCollapseRow (new CollapseRow { Text = header, Details = count.ToString () });
		}

		protected void AddCollapseRow(CollapseRow row) {

			if (_collapseRows == null)
				_collapseRows = new List<CollapseRow>();

			BeforeAddingRow (row);

			_collapseRows.Add (row);

			currentHeaderIndex++;
			_items.Add(new Tuple<string, List<ContentRow>>(row.Text, new List<ContentRow>()));
		}

		protected void ToggleSearchBarInNavBar() {
			_tableView.Frame = new RectangleF(0, 44, View.Frame.Width, View.Frame.Height - 44);
			NavigationController.SetNavigationBarHidden (true, false);
			_navSearchBar.Hidden = false;
			_navSearchBar.BecomeFirstResponder ();
		}

		private UISearchBar _navSearchBar;

		private void SearchBarCancelClicked() {
			SearchBarTextChanged ("");
			_tableView.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
			_navSearchBar.ResignFirstResponder ();
			_navSearchBar.Hidden = true;
			_navSearchBar.Text = "";
			NavigationController.SetNavigationBarHidden (false, false);
		}

		private void InitializeSearchBarInNavBar() {
			_navSearchBar = new UISearchBar();
			_navSearchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			_navSearchBar.Frame = new RectangleF(0, 0, _tableView.Frame.Width, 44);
			_navSearchBar.SetShowsCancelButton (true, false);

			_navSearchBar.TextChanged += delegate {
				SearchBarTextChanged (_navSearchBar.Text);
			};

			_navSearchBar.CancelButtonClicked += delegate {
				SearchBarCancelClicked ();
			};

			_navSearchBar.SearchButtonClicked += delegate {
				_navSearchBar.ResignFirstResponder();
			};

			_navSearchBar.Hidden = true;
			Add (_navSearchBar);
		}

		private void InitializeStickySearchBar() {
			var searchBar = new UISearchBar();
			searchBar.Frame = new RectangleF(0, 0, _tableView.Frame.Width, 44);
			searchBar.SetShowsCancelButton (false, false);
			
			searchBar.OnEditingStarted += delegate {
				searchBar.SetShowsCancelButton (true, true);
			};
			
			searchBar.TextChanged += delegate {
				SearchBarTextChanged (searchBar.Text);
			};

			searchBar.CancelButtonClicked += delegate {
				searchBar.ResignFirstResponder ();
				searchBar.SetShowsCancelButton (false, true);
			};

			searchBar.SearchButtonClicked += delegate {
				searchBar.ResignFirstResponder();
				searchBar.SetShowsCancelButton (false, true);
			};

			_tableView.TableHeaderView = searchBar;
		}

		private void SearchBarTextChanged(string text) {

			Func<ContentRow, bool> searchFunc = row => 
				row.Text.IndexOf (text, StringComparison.OrdinalIgnoreCase) != -1 || 
				(
					row.SearchDetailsField != null ?
						(row.SearchDetailsField.IndexOf (text, StringComparison.OrdinalIgnoreCase) != -1) :
						(row.Details != null && row.Details.IndexOf (text, StringComparison.OrdinalIgnoreCase) != -1)
				);

			if (string.IsNullOrEmpty(text)) {
				_items = new List<Tuple<string, List<ContentRow>>>(_originalItems);
				if (_originalCollapseRows != null)
					_collapseRows = new List<CollapseRow> (_originalCollapseRows);
			}
			else {
				_items.Clear ();

				List<CollapseRow> leftCollapseRows = null;

				if (_originalCollapseRows != null) {
					leftCollapseRows = new List<CollapseRow> ();
				}

				foreach(var item in _originalItems) {
					var found = item.Item2.Where(searchFunc).ToList ();

					if (found.Any ()) {
						_items.Add (new Tuple<string, List<ContentRow>>(item.Item1, found));

						if (leftCollapseRows != null)  {
							var existingCollapseRow = _originalCollapseRows.FirstOrDefault (r => r.Text == item.Item1);
							if (existingCollapseRow != null && !leftCollapseRows.Contains (existingCollapseRow))
								leftCollapseRows.Add (existingCollapseRow);
						}
					}
				}

				if (leftCollapseRows != null) {
					_collapseRows = leftCollapseRows;
				}
			}

			_tableView.ReloadData();
		}

		public void ImageCallback(NSIndexPath indexPath) {
			var row = ((RichTableSource)(_tableView.Source)).GetItem (indexPath);
			if (!string.IsNullOrEmpty (row.ImageUrl)) {
				
				var cell = _tableView.CellAt (indexPath);
				
				if (cell != null) {
					row.SetImage (cell, RequestImage (row.ImageUrl, indexPath, null));
					_downloadedImages [indexPath] = true;
				}
			}
		}
		
		public UIImage RequestImage(string imageUrl, NSIndexPath indexPath, Action<NSIndexPath> callback) {
			
			return ImageDownloader.DefaultRequestImage (imageUrl, new ImageUpdater(callback, indexPath), ImageWebRequestInit);
		}

		public UIImage RequestImageOrEmpty(string imageUrl, Action<NSIndexPath> callback) {

			if (string.IsNullOrEmpty (imageUrl))
				return UIImage.FromFile (NoImagePath);

			return ImageDownloader.DefaultRequestImage (imageUrl, new ImageUpdater(callback), ImageWebRequestInit);
		}
		
		public class ImageUpdater : IImageUpdated {
			
			private Action<NSIndexPath> _callback;
			private NSIndexPath _indexPath;
			
			public ImageUpdater (Action<NSIndexPath> callback)
			{
				_callback = callback;
			}
			
			public ImageUpdater (Action<NSIndexPath> callback, NSIndexPath indexPath)
			{
				_indexPath = indexPath;
				_callback = callback;
			}
			
			public void UpdatedImage (string uri)
			{
				if (_callback != null)
					_callback(_indexPath);
			}			
		}

		private class RichTableSource : UITableViewSource {
			
			private RichListController _controller;

			public RichTableSource (RichListController controller)
			{
				_controller = controller;
				
				if (_controller.SmartImageDownload)
					_controller._downloadedImages = new Dictionary<NSIndexPath, bool>();
			}

			private string[] _sections;
			public override int NumberOfSections (UITableView tableView)
			{
				if (_controller._isReloading)
					return 0;

				if (_controller.UseIndex) {
					_sections = _controller._items.Select (m => m.Item1).ToArray ();
					return _sections.Length;
				} else {
					return _controller._items.Count;
				}
			}
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				if (_controller._isReloading)
					return null;

				if (_controller.CollapseEnabled)
					return null;

				if (_controller.UseIndex) {
					
					string letter = _sections[section];
					int numberOfPeople = _controller._items[section].Item2.Count;
					if (numberOfPeople == 1)
						return letter + " - 1 person";
					else
						return string.Format("{0} - {1} people", letter, numberOfPeople);
					
				} else {
					var item = _controller._items [section];
					if (item.Item2.Any ())
						return item.Item1;
					else 
						return null;
				}
			}
			
			[Export ("sectionIndexTitlesForTableView:")]
			public NSArray SectionTitles ()
			{   
				if (_controller._isReloading)
					return null;
				
				if (_controller.UseIndex) {
					return NSArray.FromStrings (_sections);
				}
				else 
					return null;
			}

			public override float GetHeightForHeader (UITableView tableView, int section)
			{
				if (_controller.GetHeightForHeader != null)
					return _controller.GetHeightForHeader (section);

				if (string.IsNullOrWhiteSpace (_controller._items [section].Item1))
					return 0;
				else if (_controller.CollapseEnabled)
					return 0;
				else
					return _controller.HeaderHeight;
			}

			public override UIView GetViewForHeader (UITableView tableView, int section)
			{
				if (!IsIOS5() && _controller.SectionHeaderColor != null) {
					var reuse = new NSString ("customsectionheadercell");
					var view = tableView.DequeueReusableHeaderFooterView (reuse);

					if (view == null) {
						view = new UITableViewHeaderFooterView (reuse);
						view.BackgroundView = new UIView { BackgroundColor = _controller.SectionHeaderColor };
					}

					return view;
				} else {
					return null;
				}
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				if (_controller._isReloading)
					return 0;

				int count = _controller._items[section].Item2.Count;
				
				if (_controller.EnablePaging && _controller.HasMorePages && section == _controller.PagingSectionIndex)
					count++;

				if (_controller.CollapseEnabled)
					count++;

				return count;
			}
			
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				return _controller.EnableEditing;
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (_controller._isReloading)
					return;
				
				if (!_controller.EnableEditing)
					return;

				var item = GetItem (indexPath);

				if (item == null)
					return;

				if (editingStyle == UITableViewCellEditingStyle.Delete && item.DeleteAction != null)
					item.DeleteAction ();
			}

			private UITableViewCell EmptyCell() {
				var cell = _controller._tableView.DequeueReusableCell ("emptycell");
				if (cell == null)
					cell = new UITableViewCell (UITableViewCellStyle.Default, "emptycell");
				
				return cell;
			}

			public override int IndentationLevel (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller.CollapseEnabled && indexPath.Row > 0)
					return _controller.CollapseIndentLevel;
				else 
					return 0;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller._isReloading)
					return EmptyCell();
				
                if (_controller.EnablePaging && 
                    _controller.HasMorePages && 
                    _controller._items[indexPath.Section].Item2.Count == indexPath.Row &&
                    indexPath.Section == _controller.PagingSectionIndex)
                {
                    return BuildNextResultsCell("Load next page", tableView.Frame.Width);
                }

				if (_controller.CollapseEnabled && indexPath.Row == 0)
					return _controller._collapseRows[indexPath.Section].GetCell (tableView, indexPath);

				var item = GetItem (indexPath);
				var cell = item.GetCell(tableView, indexPath);
				item.AfterGetCell (tableView, cell, indexPath);
				return cell;
			}
			
			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller._isReloading)
					return 0;
				
				if (_controller.EnablePaging && _controller.HasMorePages && _controller._items [indexPath.Section].Item2.Count == indexPath.Row)
					return 44;

				if (_controller.CollapseEnabled && indexPath.Row == 0) {
					if (string.IsNullOrEmpty (_controller._collapseRows [indexPath.Section].Text))
						return 0;
					else 
						return 32;
				}

				var item = GetItem (indexPath);

				if (item != null)
					return item.GetHeight (tableView);
				else 
					return 44;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller._isReloading)
					return;

				if (_controller.EnablePaging && _controller.HasMorePages && _controller._items [indexPath.Section].Item2.Count == indexPath.Row) {
					StartAnimating ();
					_controller.PagingAction();
					return;
				}

				if (_controller.CollapseEnabled && indexPath.Row == 0) {
					var row = _controller._collapseRows [indexPath.Section];
					if (row.Action != null) {
						row.Action ();
					}
					else if (row.NavController != null) {
						var newController = row.NavController();

						if (newController != null)
							RichListController.PushAction (_controller, newController);
					}
					else {
						var cell = ((CollapseCell)tableView.CellAt (indexPath));
						cell.ToggleCollapse ();
					}
					return;
				}

				var item = GetItem (indexPath); 

				if (item == null)
					return;

				if (tableView.Editing)
					item.RowSelectedInEdit (tableView);
				else 
					item.RowSelected(tableView);
			}

			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller._isReloading)
					return;

				var item = GetItem (indexPath);

				if (item == null)
					return;

				if (item.AccessoryAction != null) {
					item.AccessoryAction ();
				}
			}

			public ContentRow GetItem(NSIndexPath indexPath) {

				int row = indexPath.Row;
				if (_controller.CollapseEnabled && row > 0)
					row--;

				var items = _controller._items [indexPath.Section].Item2;
				if (items.Count <= row)
					return null;
				else 
					return items[row];
			}

            public override void Scrolled(UIScrollView scrollView)
            {
                if (scrollView.Dragging && _controller.HasMorePages && _controller.EnableInfinitePaging)
                {
                    float threshold = scrollView.ContentSize.Height - scrollView.Bounds.Height;
                    if (scrollView.ContentOffset.Y > threshold - 160 && !_controller._pagingActionInProgress)
                    {
                        Console.WriteLine("starting infinite loading");
                        _controller._pagingActionInProgress = true;


                        var existingInset = scrollView.ContentInset;
                        float bottom = existingInset.Bottom;
                        existingInset.Bottom += 60;
                        scrollView.ContentInset = existingInset;

                        var view = new UIView(new RectangleF(0, scrollView.ContentSize.Height, scrollView.Bounds.Width, 60));
                        scrollView.Add(view);

                        var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray); 
                        indicator.Center = new PointF(view.Bounds.Width / 2, view.Bounds.Height / 2);
                        indicator.HidesWhenStopped = true;
                        indicator.StartAnimating();
                        view.Add(indicator);

                        _controller._tableView.BeginUpdates();
                        int position = _controller._items[0].Item2.Count;

                        _controller.InfinitePagingAction(addedCount => {
                            Console.WriteLine ("inside infinite paging action");
                            indicator.StopAnimating();
                            view.RemoveFromSuperview();
                            var ex = scrollView.ContentInset;
                            ex.Bottom = bottom;
                            scrollView.ContentInset = ex;

                            _controller._tableView.InsertRows(Enumerable.Range(position, addedCount).Select(r => NSIndexPath.FromRowSection(r, _controller.PagingSectionIndex)).ToArray(), UITableViewRowAnimation.Fade);
                            _controller._tableView.EndUpdates();

                            NSTimer.CreateScheduledTimer(1, delegate {
                                _controller._pagingActionInProgress = false;
                            });
                        });
                    }
                }
            }

			public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
			{
				if (_controller._isReloading)
					return;
				
				if (!willDecelerate && _controller.SmartImageDownload)
					LoadImagesForOnscreenRows ();
			}
			
			public override void DecelerationEnded (UIScrollView scrollView)
			{
				if (_controller._isReloading)
					return;
				
				if (_controller.SmartImageDownload)
					LoadImagesForOnscreenRows ();
			}
			
			private void LoadImagesForOnscreenRows ()
			{
				if (_controller._isReloading)
					return;
				
				if (!_controller.SmartImageDownload)
					return;
				
				var rows = _controller._tableView.IndexPathsForVisibleRows;
				
				foreach (NSIndexPath path in rows) {

					if (_controller.CollapseEnabled && path.Row == 0)
						continue;

					if (_controller.EnablePaging && _controller.HasMorePages && _controller._items [path.Section].Item2.Count == path.Row)
						continue;
					
					var item = GetItem (path);

					if (item == null)
						continue;

					if (item.ImageUrl == null) 
						continue;
					
					if (_controller._downloadedImages.ContainsKey(path) && _controller._downloadedImages[path]) { 
						continue;
					}
					
					var cachedImage = _controller.ImageCacheGetter(item.ImageUrl);
					if (cachedImage == null) {
						_controller.RequestImage(item.ImageUrl, path, _controller.ImageCallback);
					}
				}
			}

			private UIActivityIndicatorView _pagingActivity;
			private UITableViewCell _pagingCell;
			
			private UITableViewCell BuildNextResultsCell(string text, float tableWidth)
			{
				var cell = new UITableViewCell(UITableViewCellStyle.Default, "lastcell");
				cell.TextLabel.Text = text;
				cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(15);
				cell.TextLabel.TextAlignment = UITextAlignment.Center;
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				
				if (_pagingActivity != null)
					_pagingActivity.RemoveFromSuperview();
				
				_pagingActivity = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray); 
				_pagingActivity.Frame = new RectangleF((tableWidth - 32) / 2, 4, 32, 32);
				_pagingActivity.HidesWhenStopped = true;
				cell.AddSubview(_pagingActivity);
				_pagingCell = cell;
				return cell;
			}
            			
			private void StartAnimating() {
				if (_pagingActivity != null) {
					_pagingCell.TextLabel.Text = "";
					_pagingActivity.Hidden = false;
					_pagingActivity.StartAnimating();
				}
			}
			
			private void StopAnimating() {
				if (_pagingActivity != null && _pagingActivity.IsAnimating) {
					_pagingActivity.StopAnimating();
				}
			}

			private static bool IsIOS5() {
				return UIDevice.CurrentDevice.SystemVersion.Split ('.') [0] == "5";
			}
		}
	}

	public class LoadingDataSource : UITableViewSource {
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return 1;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = new UITableViewCell(UITableViewCellStyle.Default, "loadingcell");	
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			cell.AddSubview(CellActivity(tableView));
			return cell;
		}
		
		private static UIActivityIndicatorView CellActivity(UITableView tableView) {
			
			var activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
			activityView.Frame = new RectangleF((tableView.Frame.Width - 28) / 2, 8, 28, 28);
			activityView.HidesWhenStopped = true;
			activityView.StartAnimating();
			return activityView;
		}
	}

	public class NoResultsDataSource : UITableViewSource {
		
		private readonly string _textMessage;
		
		private UIFont _font = UIFont.BoldSystemFontOfSize(15);
		
		public NoResultsDataSource (string textMessage)
		{
			_textMessage = textMessage;
		}
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return 1;
		}
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 44;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = new UITableViewCell(UITableViewCellStyle.Default, "noresultscell");	
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			cell.TextLabel.Text = _textMessage;
			cell.TextLabel.TextAlignment = UITextAlignment.Center;
			cell.TextLabel.Font = _font;
			cell.TextLabel.Lines = 0;
			
			foreach(var view in cell.ContentView.Subviews)
				view.BackgroundColor = UIColor.Clear;
			
			return cell;
		}
	}

    public class TryAgainDataSource : UITableViewSource {

        private readonly string _textMessage;
        private readonly Action _reloadAction;

        public TryAgainDataSource (string textMessage, Action reloadAction)
        {
            _textMessage = textMessage;
            _reloadAction = reloadAction;
        }

        public override int RowsInSection (UITableView tableview, int section)
        {
            return 1;
        }

        public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
        {
            return 44;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (_reloadAction != null)
            {
                tableView.Source = new LoadingDataSource();
                tableView.ReloadData();
                _reloadAction();
            }
        }

        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "tryagaincell");  

            var label1 = new UILabel();
            label1.Text = _textMessage;
            label1.TextColor = UIColor.Red;
            label1.TextAlignment = UITextAlignment.Center;
            label1.Font = UIFont.BoldSystemFontOfSize(15);
            cell.ContentView.Add(label1);

            var label2 = new UILabel();
            label2.Text = "(tap to reload)";
            label2.TextColor = UIColor.Gray;
            label2.Font = UIFont.SystemFontOfSize(13);
            label2.TextAlignment = UITextAlignment.Center;
            cell.ContentView.Add(label2);

            label1.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width, 24);
            label2.Frame = new RectangleF(0, 24, cell.ContentView.Frame.Width, 20);

            return cell;
        }
    }
}