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
using FieldService.Utilities;

namespace FieldService.iOS
{
	/// <summary>
	/// This controller holds the main menu for assignments
	/// </summary>
	public partial class MenuController : UIViewController
	{
		public MenuController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//UI we have to setup from code
			View.BackgroundColor = Theme.LinenLeft;
			tableView.Source = new DataSource ();

			//We use a button here, because we have to set custom values UIBarButtonItem doesn't support
			var button = UIButton.FromType (UIButtonType.Custom);
			button.Font = Theme.BoldFontOfSize (14);
			button.SetTitle ("Assignments", UIControlState.Normal);
			button.SetTitleColor (UIColor.White, UIControlState.Normal);
			button.SetBackgroundImage (Theme.BackButton, UIControlState.Normal);
			button.Frame = new RectangleF (0, 0, 120, 30);

			var backButton = new UIBarButtonItem (button); 
			button.TouchUpInside += (sender, e) => {
				var window = ServiceContainer.Resolve<UIWindow> ();
				window.RootViewController = ServiceContainer.Resolve<AssignmentsController> ();
			};
			navigationBar.TopItem.LeftBarButtonItem = backButton;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			tableView.SelectRow (NSIndexPath.FromRowSection (0, 0), false, UITableViewScrollPosition.Top);
		}

		private class DataSource : UITableViewSource
		{
			readonly UITableViewCell summaryCell, mapCell, itemsCell, laborCell, expensesCell, confirmationCell;

			public DataSource ()
			{
				summaryCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				summaryCell.TextLabel.Text = "Summary";
				summaryCell.BackgroundColor = UIColor.Clear;	
				summaryCell.BackgroundView = new UIImageView { Image = Theme.LeftListTop };
				summaryCell.SelectedBackgroundView = new UIImageView { Image = Theme.LeftListTopActive };

				mapCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				mapCell.TextLabel.Text = "Map";
				mapCell.BackgroundColor = UIColor.Clear;	
				mapCell.BackgroundView = new UIImageView { Image = Theme.LeftListMid };
				mapCell.SelectedBackgroundView = new UIImageView { Image = Theme.LeftListMidActive };

				itemsCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				itemsCell.TextLabel.Text = "Items";
				itemsCell.BackgroundColor = UIColor.Clear;	
				itemsCell.BackgroundView = new UIImageView { Image = Theme.LeftListMid };
				itemsCell.SelectedBackgroundView = new UIImageView { Image = Theme.LeftListMidActive };

				laborCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				laborCell.TextLabel.Text = "Labor Hours";
				laborCell.BackgroundColor = UIColor.Clear;	
				laborCell.BackgroundView = new UIImageView { Image = Theme.LeftListMid };
				laborCell.SelectedBackgroundView = new UIImageView { Image = Theme.LeftListMidActive };

				expensesCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				expensesCell.TextLabel.Text = "Expenses";
				expensesCell.BackgroundColor = UIColor.Clear;
				expensesCell.BackgroundView = new UIImageView { Image = Theme.LeftListMid };
				expensesCell.SelectedBackgroundView = new UIImageView { Image = Theme.LeftListMidActive };

				confirmationCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				confirmationCell.TextLabel.Text = "Confirmations";
				confirmationCell.BackgroundColor = UIColor.Clear;	
				confirmationCell.BackgroundView = new UIImageView { Image = Theme.LeftListEnd };
				confirmationCell.SelectedBackgroundView = new UIImageView { Image = Theme.LeftListEndActive };
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return 6;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				switch (indexPath.Row) {
				case 0:
					return summaryCell;
				case 1:
					return mapCell;
				case 2:
					return itemsCell;
				case 3:
					return laborCell;
				case 4:
					return expensesCell;
				case 5:
					return confirmationCell;
				default:
					throw new IndexOutOfRangeException ();
				}
			}
		}
	}
}
