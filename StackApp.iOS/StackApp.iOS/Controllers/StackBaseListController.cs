using System;
using System.Threading.Tasks;
using Abilitics.iOS;
using Stacklash.Core;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Linq;

namespace Stacklash.iOS
{
    public abstract class StackBaseListController<T> : RichListController {
        protected int _page = 1;

        protected virtual Task<BaseResponse<T>> GetStackData() {
            throw new NotImplementedException();
        }

        protected virtual ContentRow GetRow(T item) {
            throw new NotImplementedException();
        }

        public override void ViewDidLoad()
        {
            //if (IOS.IsIOS7)
                //EdgesForExtendedLayout = UIRectEdge.None;

            IsPlainTable = true;
            LongRunning = true;
            EnableInfinitePaging = true;

            InfinitePagingAction = async finished => 
            {
                _page++;

                try {
                    var response = await GetStackData();

                    if (!response.Success) {
                        return;
                    }

                    HasMorePages = response.has_more;

                    foreach (T item in response.items)
                        AddRow(GetRow(item));

                    finished(response.items.Count);
                }
                catch (Exception ex) {
                    this.UnhandledError(ex);
                }

            };

            base.ViewDidLoad();

            var refresh = new UIRefreshControl();
            refresh.AttributedTitle = new NSAttributedString("Pull to refresh");
            _tableView.Add(refresh);
            refresh.AddTarget(async delegate {
                await ReloadAllData();
                refresh.EndRefreshing();
                _page = 1;
            }, UIControlEvent.ValueChanged);
        }

        public async override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            await ReloadAllData();
        }

        protected virtual void BeforeAddingRows() {
            // do nothing
        }

        protected virtual void EmptyDataSetBehavior() {
            // do nothing
        }

        protected async Task ReloadAllData() {
            try {
                var response = await GetStackData();
                ClearAndPopulateData(response);
            }
            catch (Exception ex) {
                this.UnhandledError(ex);
            }
        }

        protected void ClearAndPopulateData(BaseResponse<T> response) {
            if (!response.Success) {
                _tableView.Source = new TryAgainDataSource(response.ErrorMessage, async delegate { await ReloadAllData(); });
                _tableView.ReloadData();
                return;
            }

            HasMorePages = response.has_more;

            ClearAllRows();
            BeforeAddingRows();

            if (response.items == null || !response.items.Any())
                EmptyDataSetBehavior();

            foreach (T item in response.items)
                AddRow(GetRow(item));

            ForceReloadData();
        }
    }
}

