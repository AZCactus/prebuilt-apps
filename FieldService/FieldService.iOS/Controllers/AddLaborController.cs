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
//    limitations under the License.using System;
using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FieldService.Utilities;
using FieldService.Data;
using FieldService.ViewModels;

namespace FieldService.iOS
{
	/// <summary>
	/// Controller for manually adding labor to an assignment
	/// </summary>
	public partial class AddLaborController : BaseController
	{
		readonly LaborController laborController;
		readonly AssignmentDetailsController detailController;
		readonly LaborViewModel laborViewModel;
		UIBarButtonItem labor, space1, space2, done;
		TableSource tableSource;

		public AddLaborController (IntPtr handle) : base (handle)
		{
			ServiceContainer.Register (this);

			laborViewModel = new LaborViewModel();
			laborController = ServiceContainer.Resolve<LaborController>();
			detailController = ServiceContainer.Resolve<AssignmentDetailsController>();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//UI setup from code
			cancel.SetTitleTextAttributes (new UITextAttributes() { TextColor = UIColor.White }, UIControlState.Normal);
			cancel.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);
			
			var label = new UILabel (new RectangleF(0, 0, 80, 36)) { 
				Text = "Labor",
				TextColor = UIColor.White,
				BackgroundColor = UIColor.Clear,
				Font = Theme.BoldFontOfSize (18),
			};
			labor = new UIBarButtonItem(label);

			done = new UIBarButtonItem("Done", UIBarButtonItemStyle.Bordered, (sender, e) => {
				laborViewModel
					.SaveLaborAsync (detailController.Assignment, laborController.Labor)
					.ContinueOnUIThread (_ => DismissViewController (true, delegate { }));
			});
			done.SetTitleTextAttributes (new UITextAttributes() { TextColor = UIColor.White }, UIControlState.Normal);
			done.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);
			
			space1 = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			space2 = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

			tableView.Source = 
				tableSource = new TableSource();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			//Load labor hours for the table
			bool enabled = detailController.Assignment.Status != AssignmentStatus.Complete;
			if (enabled) {
				toolbar.Items = new UIBarButtonItem[] {
					cancel,
					space1,
					labor,
					space2,
					done,
				};
			} else {
				toolbar.Items = new UIBarButtonItem[] { cancel, space1, labor, space2 };
			}
			tableSource.Load (enabled, laborController.Labor);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			//Reload the labor on the other screen when dismissed
			laborController.ReloadLabor ();
		}

		/// <summary>
		/// Event when cancel button is clicked
		/// </summary>
		partial void Cancel (NSObject sender)
		{
			DismissViewController (true, delegate {	});
		}

		/// <summary>
		/// The table source - has static cells
		/// </summary>
		private class TableSource : UITableViewSource
		{
			readonly LaborController laborController;
			readonly UITableViewCell typeCell, hoursCell, descriptionCell;
			readonly UILabel type;
			readonly PlaceholderTextView description;
			readonly HoursField hours;
			LaborTypeSheet laborSheet;
			bool enabled;
			
			public TableSource ()
			{
				laborController = ServiceContainer.Resolve<LaborController>();

				typeCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				typeCell.TextLabel.Text = "Type";
				typeCell.AccessoryView = type = new UILabel (new RectangleF(0, 0, 200, 36))
				{
					TextAlignment = UITextAlignment.Right,
					BackgroundColor = UIColor.Clear,
				};
				typeCell.SelectionStyle = UITableViewCellSelectionStyle.None;

				hoursCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				hoursCell.TextLabel.Text = "Hours";
				hoursCell.SelectionStyle = UITableViewCellSelectionStyle.None;
				hoursCell.AccessoryView = hours = new HoursField(new RectangleF(0, 0, 200, 44));
				hours.ValueChanged += (sender, e) => laborController.Labor.Hours = TimeSpan.FromHours (hours.Value);

				descriptionCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				descriptionCell.AccessoryView = description = new PlaceholderTextView(new RectangleF(0, 0, 470, 400))
				{
					BackgroundColor = UIColor.Clear,
					TextColor = Theme.LabelColor,
					Placeholder = "Please enter notes here",
				};
				descriptionCell.SelectionStyle = UITableViewCellSelectionStyle.None;
				description.SetDidChangeNotification (d => {
					if (description.Text != description.Placeholder) {
						laborController.Labor.Description = d.Text;
					} else {
						laborController.Labor.Description = string.Empty;
					}
				});
			}

			public void Load (bool enabled, Labor labor)
			{
				this.enabled = enabled;

				type.Enabled =
					hours.Enabled =
					description.UserInteractionEnabled = enabled;

				type.Text = labor.TypeAsString;
				hours.Value = labor.Hours.TotalHours;
				description.Text = string.IsNullOrEmpty (labor.Description) ? description.Placeholder : labor.Description;
			}

			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return indexPath.Section == 1 ? 410 : 44;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return 2;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				return section == 0 ? 2 : 1;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (enabled) {
					if (indexPath.Section == 0) {
						if (indexPath.Row == 0) {
							//Type changed
							laborSheet = new LaborTypeSheet ();
							laborSheet.Dismissed += (sender, e) => {
								var labor = laborController.Labor;
								if (laborSheet.Type.HasValue && labor.Type != laborSheet.Type) {
									labor.Type = laborSheet.Type.Value;

									Load (enabled, labor);
								}

								laborSheet.Dispose ();
								laborSheet = null;
							};
							laborSheet.ShowFrom (typeCell.Frame, tableView, true);
						} else {
							//Give hours "focus"
							hours.BecomeFirstResponder ();
						}
					} else {
						//Give description "focus"
						description.BecomeFirstResponder ();
					}
				}
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					return indexPath.Row == 0 ? typeCell : hoursCell;
				} else {
					return descriptionCell;
				}
			}
			
			protected override void Dispose (bool disposing)
			{
				typeCell.Dispose ();
				hoursCell.Dispose ();
				descriptionCell.Dispose ();
				type.Dispose ();
				description.Dispose ();
				hours.Dispose ();
				
				base.Dispose (disposing);
			}
		}
	}
}
