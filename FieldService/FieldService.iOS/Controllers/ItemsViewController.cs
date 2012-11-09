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
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FieldService.ViewModels;
using FieldService.Utilities;
using FieldService.Data;

namespace FieldService.iOS
{
	/// <summary>
	/// Controller for the list of items
	/// </summary>
	public partial class ItemsViewController : BaseController
	{
		readonly AssignmentDetailsController detailsController;
		readonly ItemViewModel itemViewModel;
		UILabel title;
		UIBarButtonItem titleButton, edit, addItem, space;

		public ItemsViewController (IntPtr handle) : base (handle)
		{
			ServiceContainer.Register (this);

			itemViewModel = ServiceContainer.Resolve<ItemViewModel> ();
			detailsController = ServiceContainer.Resolve<AssignmentDetailsController> ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//UI that must be setup from code
			View.BackgroundColor = Theme.LinenPattern;

			title = new UILabel (new RectangleF (0, 0, 100, 36)) { 
				TextColor = UIColor.White,
				BackgroundColor = UIColor.Clear,
				Font = Theme.BoldFontOfSize (16),
				Text = "Items",
			};
			titleButton = new UIBarButtonItem (title);
			toolbar.Items = new UIBarButtonItem[] { titleButton };

			var textAttributes = new UITextAttributes { TextColor = UIColor.White };
			edit = new UIBarButtonItem ("Edit", UIBarButtonItemStyle.Bordered, delegate {
				edit.Title = tableView.Editing ? "Edit" : "Done";
				tableView.SetEditing (!tableView.Editing, true);
			});
			edit.SetTitleTextAttributes (textAttributes, UIControlState.Normal);
			edit.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);

			space = new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace);

			addItem = new UIBarButtonItem ("Add Item", UIBarButtonItemStyle.Bordered, (sender, e) => PerformSegue ("AddItem", this));
			addItem.SetTitleTextAttributes (textAttributes, UIControlState.Normal);
			addItem.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);

			tableView.Source = new TableSource ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			//Reload our list of items
			ReloadItems ();
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			//Dismiss editing on the table view
			if (tableView.Editing) {
				edit.Title = "Edit";
				tableView.SetEditing (false, true);
			}
		}

		/// <summary>
		/// Call to reload the list of items
		/// </summary>
		public void ReloadItems ()
		{
			if (IsViewLoaded) {

				if (detailsController.Assignment.Status == AssignmentStatus.Complete) {
					toolbar.Items = new UIBarButtonItem[] { titleButton };
				} else {
					toolbar.Items = new UIBarButtonItem[] {
						titleButton,
						space,
						edit,
						addItem
					};
				}

				itemViewModel.LoadAssignmentItems (detailsController.Assignment)
					.ContinueOnUIThread (_ => {
					if (itemViewModel.AssignmentItems == null || itemViewModel.AssignmentItems.Count == 0) 
						title.Text = "Items";
					else
						title.Text = string.Format ("Items ({0})", itemViewModel.AssignmentItems.Count);
					tableView.ReloadData ();
				});
			}
		}

		/// <summary>
		/// Table source of items
		/// </summary>
		private class TableSource : UITableViewSource
		{
			readonly ItemViewModel itemViewModel;
			readonly ItemsViewController itemController;
			readonly AssignmentDetailsController detailsController;
			const string Identifier = "AssignmentItemCell";

			public TableSource ()
			{
				itemController = ServiceContainer.Resolve<ItemsViewController> ();
				detailsController = ServiceContainer.Resolve <AssignmentDetailsController> ();
				itemViewModel = ServiceContainer.Resolve<ItemViewModel> ();
			}

			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				return detailsController.Assignment.Status != AssignmentStatus.Complete;
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				itemViewModel
					.DeleteAssignmentItem (detailsController.Assignment, itemViewModel.AssignmentItems [indexPath.Row])
					.ContinueOnUIThread (_ => itemController.ReloadItems ());
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.CellAt (indexPath) as AssignmentItemCell;
				cell.Checked ();
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return itemViewModel.AssignmentItems == null ? 0 : itemViewModel.AssignmentItems.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (Identifier) as AssignmentItemCell;
				cell.SetItem (itemViewModel.AssignmentItems [indexPath.Row]);
				return cell;
			}
		}
	}
}
