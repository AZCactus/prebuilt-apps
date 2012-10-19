//
//  Copyright 2012  Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Linq;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FieldService.Data;
using FieldService.ViewModels;
using FieldService.Utilities;

namespace FieldService.iOS
{
	/// <summary>
	/// Modal view controller for adding items to an assignment
	/// </summary>
	public partial class AddItemController : UIViewController
	{
		readonly ItemViewModel itemViewModel;

		public AddItemController (IntPtr handle) : base (handle)
		{
			ServiceContainer.Register (this);

			itemViewModel = ServiceContainer.Resolve <ItemViewModel>();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			cancel.SetTitleTextAttributes (new UITextAttributes() { TextColor = UIColor.White }, UIControlState.Normal);
			cancel.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);

			var label = new UILabel (new RectangleF(0, 0, 80, 36)) { 
				Text = "Items",
				TextColor = UIColor.White,
				BackgroundColor = UIColor.Clear,
				Font = Theme.BoldFontOfSize (16),
			};
			var items = new UIBarButtonItem(label);

			toolbar.Items = new UIBarButtonItem[] {
				cancel,
				new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
				items,
				new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
			};

			tableView.Source = new TableSource();
			var searchDataSource = new SearchSource ();
			SearchDisplayController.SearchResultsSource = searchDataSource;
			SearchDisplayController.Delegate = new SearchDisplay (tableView, searchDataSource);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			itemViewModel.LoadItems ().ContinueOnUIThread (_ => tableView.ReloadData ());
		}

		partial void Cancel (NSObject sender)
		{
			DismissViewController (true, delegate {	});
		}

		/// <summary>
		/// Table source for the list of items
		/// </summary>
		private class TableSource : UITableViewSource
		{
			const string Identifier = "ItemCell";
			readonly AddItemController addItemController;
			readonly ItemsViewController itemController;
			readonly AssignmentDetailsController detailController;
			protected readonly ItemViewModel itemViewModel;

			public TableSource ()
			{
				addItemController = ServiceContainer.Resolve <AddItemController>();
				itemController = ServiceContainer.Resolve <ItemsViewController>();
				detailController = ServiceContainer.Resolve <AssignmentDetailsController>();
				itemViewModel = ServiceContainer.Resolve <ItemViewModel>();
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return itemViewModel.Items == null ? 0 : itemViewModel.Items.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var item = GetItem (indexPath);
				var cell = tableView.DequeueReusableCell (Identifier);
				if (cell == null)
				{
					cell = new UITableViewCell(UITableViewCellStyle.Default, Identifier);
				}
				cell.TextLabel.Text = item.Name + " " + item.Number;
				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				var item = GetItem (indexPath);
				tableView.UserInteractionEnabled = false;
				itemViewModel.SaveAssignmentItem (detailController.Assignment, new AssignmentItem {
					Item = item.ID,
					Assignment = detailController.Assignment.ID,
				})
				.ContinueOnUIThread(_ => {
					tableView.UserInteractionEnabled = true;
					itemController.ReloadItems ();
					addItemController.DismissViewController (true, delegate { });
				});
			}

			protected virtual Item GetItem(NSIndexPath indexPath)
			{
				return itemViewModel.Items[indexPath.Row];
			}
		}

		/// <summary>
		/// Search source for the list of items
		/// </summary>
		private class SearchSource : TableSource, ISearchSource
		{
			public string SearchText
			{
				get;
				set;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return itemViewModel.Items == null ? 0 : itemViewModel.Items.Count (Filter);
			}

			protected override Item GetItem (NSIndexPath indexPath)
			{
				return itemViewModel.Items.Where (Filter).Skip (indexPath.Row).First ();
			}

			private bool Filter(Item item)
			{
				return !string.IsNullOrEmpty (SearchText) && 
					(item.Name.ToLower ().StartsWith (SearchText) || item.Number.ToLower().StartsWith(SearchText));
			}
		}
	}
}
