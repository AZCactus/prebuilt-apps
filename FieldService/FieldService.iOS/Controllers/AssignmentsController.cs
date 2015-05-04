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
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using UIKit;

using FieldService.ViewModels;
using FieldService.Data;
using FieldService.Utilities;

namespace FieldService.iOS
{
	/// <summary>
	/// Controller for the main list of assignments
	/// </summary>
	public partial class AssignmentsController : BaseController
	{
		readonly AssignmentViewModel assignmentViewModel;
		bool activeAssignmentVisible = true;

		public AssignmentsController (IntPtr handle) : base (handle)
		{
			assignmentViewModel = ServiceContainer.Resolve<AssignmentViewModel>();

			assignmentViewModel.HoursChanged += OnHoursChanged;;
			assignmentViewModel.RecordingChanged += OnRecordingChanged;
		}

		void OnHoursChanged (object sender, EventArgs e)
		{
			if (IsViewLoaded)
				timerLabel.Text = assignmentViewModel.Hours.ToString (@"hh\:mm\:ss");
		}

		void OnRecordingChanged (object sender, EventArgs e)
		{
			if (IsViewLoaded)
				record.SetImage (assignmentViewModel.Recording ? Theme.RecordActive : Theme.Record, UIControlState.Normal);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			//Do this because the ViewModel hangs around for the lifetime of the app
			assignmentViewModel.HoursChanged -= OnHoursChanged;;
			assignmentViewModel.RecordingChanged -= OnRecordingChanged;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//Setup UI that is required from code
			tableView.Source = new TableSource (this);
			assignmentButton.SetBackgroundImage (Theme.AssignmentActive, UIControlState.Normal);
			contact.IconImage = Theme.IconPhone;
			address.IconImage = Theme.Map;
			priority.TextColor = UIColor.White;
			priorityBackground.Image = Theme.NumberBox;
			record.ContentMode = UIViewContentMode.Center;
			record.SetImage (assignmentViewModel.Recording ? Theme.RecordActive : Theme.Record, UIControlState.Normal);

			timerLabel.TextColor =
				numberAndDate.TextColor =
				titleLabel.TextColor =
				startAndEnd.TextColor = Theme.LabelColor;

			status.StatusChanged += (sender, e) => {
				assignmentViewModel
					.SaveAssignmentAsync (assignmentViewModel.ActiveAssignment)
					.ContinueWith (_ => BeginInvokeOnMainThread (ReloadAssignments));
			};
			status.Completed += (sender, e) => {
				var menuViewModel = ServiceContainer.Resolve<MenuViewModel>();
				menuViewModel.MenuIndex = SectionIndex.Confirmations;
				assignmentViewModel.SelectedAssignment = status.Assignment;
				PerformSegue ("AssignmentDetails", this);
			};

			//Start the active assignment out as not visible
			SetActiveAssignmentVisible (false, false);

			//Load the current timer status
			record.Enabled = false;
			assignmentViewModel.LoadTimerEntryAsync ().ContinueWith (_ => {
				BeginInvokeOnMainThread (() => {
					record.Enabled = true;
					if (assignmentViewModel.Recording)
						record.SetImage (Theme.RecordActive, UIControlState.Normal);
					else
						record.SetImage (Theme.Record, UIControlState.Normal);
				});
			});

			if (!Theme.IsiOS7) {
				tableView.BackgroundColor = Theme.BackgroundColor;
				assignmentButton.SetBackgroundImage (Theme.AssignmentActiveBlue, UIControlState.Highlighted);
				toolbarShadow.Image = Theme.ToolbarShadow;
				timerBackgroundImage.Image = Theme.TimerField;
				return;
			}

			tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
			timerLabel.Font = Theme.FontOfSize (16f);
			startAndEnd.Font = Theme.BoldFontOfSize (10f);
			startAndEnd.TextColor = UIColor.White;

			//Shadow frame
			var frame = toolbarShadow.Frame;
			frame.Height = 1f;
			toolbarShadow.Frame = frame;
			toolbarShadow.Image = UIColor.LightGray.ToImage ();

			//Status dropdown frame
			frame = status.Frame;
			frame.Width /= 2f;
			frame.X += frame.Width + 9f;
			status.Frame = frame;

			const float offset = 100f;

			//Timer frame
			frame = timerLabel.Frame;
			frame.X += offset + 35f;
			timerLabel.Frame = frame;

			//Record (play/pause) button frame
			frame = record.Frame;
			frame.X += offset;
			record.Frame = frame;

			//Priority frames
			frame = priorityBackground.Frame;
			frame.X -= 10f;
			frame.Width = frame.Height;
			priorityBackground.Frame =
				priority.Frame = frame;

			//Info frames
			frame = numberAndDate.Frame;
			frame.X -= 10f;
			numberAndDate.Frame = frame;

			frame = titleLabel.Frame;
			frame.X -= 10f;
			titleLabel.Frame = frame;

			frame = startAndEnd.Frame;
			frame.X -= 6f;
			startAndEnd.Frame = frame;

			//Address frame
			frame = address.Frame;
			frame.X -= 10f;
			address.Frame = frame;

			//Contact frame
			frame = contact.Frame;
			frame.X -= 10f;
			contact.Frame = frame;

			//Additional green rectangle on the right
			var statusView = new UIView (new CGRect (activeAssignment.Frame.Width - 8f, 0f, 8f, activeAssignment.Frame.Height)) {
				BackgroundColor = Theme.GreenColor,
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleLeftMargin,
			};
			activeAssignment.AddSubview (statusView);

			//Additional box for the start/end date
			frame = startAndEnd.Frame;
			frame.X -= 4f;
			frame.Y += 4f;
			frame.Width = 102f;
			frame.Height = 16f;
			var timeBox = new UIImageView (frame) {
				Image = Theme.TimeBox,
				ContentMode = UIViewContentMode.Left,
			};
			activeAssignment.AddSubview (timeBox);
			activeAssignment.BringSubviewToFront (startAndEnd);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			//Load our assignments
			ReloadAssignments ();
			//Apply the current orientation
			if (InterfaceOrientation.IsLandscape ()) {
				contact.Alpha = 
					address.Alpha = 1;
			} else {
				contact.Alpha = 
					address.Alpha = 0;
			}
		}

		/// <summary>
		/// We override this to show/hide some controls during rotation
		/// </summary>c
		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			bool wasPortrait = InterfaceOrientation.IsPortrait ();
			bool willBePortrait = toInterfaceOrientation.IsPortrait ();
			bool wasLandscape = InterfaceOrientation.IsLandscape ();
			bool willBeLandscape = toInterfaceOrientation.IsLandscape ();

			if (wasPortrait && willBeLandscape)
				SetContactVisible (true, duration);
			else if (wasLandscape && willBePortrait)
				SetContactVisible (false, duration);

			base.WillRotate (toInterfaceOrientation, duration);
		}

		/// <summary>
		/// This is uses to show/hide contact and address on rotation
		/// </summary>
		void SetContactVisible (bool visible, double duration)
		{
			UIView.BeginAnimations ("SetContactVisible");
			UIView.SetAnimationDuration (duration);
			UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);

			if (visible) {
				contact.Alpha = 
					address.Alpha = 1;
			} else {
				contact.Alpha = 
					address.Alpha = 0;
			}

			UIView.CommitAnimations ();
		}

		/// <summary>
		/// Reloads the entire list of assignments
		/// </summary>
		public void ReloadAssignments ()
		{
			assignmentViewModel.LoadAssignmentsAsync ().ContinueWith (_ => {
				BeginInvokeOnMainThread (() => {
					if (assignmentViewModel.ActiveAssignment == null) {
						SetActiveAssignmentVisible (false);
					} else {
						SetActiveAssignmentVisible (true);
						LoadActiveAssignment ();
					}
					
					tableView.ReloadData ();
				});
			});
		}

		/// <summary>
		/// Reloads a single row
		/// </summary>
		public void ReloadSingleRow (NSIndexPath indexPath)
		{
			tableView.ReloadRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
		}

		/// <summary>
		/// Event when the active assignment is clicked
		/// </summary>
		partial void ActiveAssignmentSelected ()
		{
			var menuViewModel = ServiceContainer.Resolve<MenuViewModel>();
			menuViewModel.MenuIndex = SectionIndex.Summary;
			assignmentViewModel.SelectedAssignment = assignmentViewModel.ActiveAssignment;
			PerformSegue ("AssignmentDetails", this);
		}

		/// <summary>
		/// Sets the visibility of the active assignment, with a nice animation
		/// </summary>
		void SetActiveAssignmentVisible (bool visible, bool animate = true)
		{
			if (visible == activeAssignmentVisible)
				return;

			if (animate) {
				UIView.BeginAnimations ("ChangeActiveAssignment");
				UIView.SetAnimationDuration (.3);
				UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);
			}

			//Modify the tableView's frame
			float height = 95;
			var frame = tableView.Frame;
			if (visible) {
				frame.Y += height;
				frame.Height -= height;
			} else {
				frame.Y -= height;
				frame.Height += height;
			}
			tableView.Frame = frame;

			//Modify the active assignment's frame
			frame = activeAssignment.Frame;
			if (visible)
				frame.Y += height;
			else
				frame.Y -= height;

			activeAssignment.Frame = frame;

			//Modify the toolbar shadow's frame
			frame = toolbarShadow.Frame;
			if (visible)
				frame.Y += height;
			else
				frame.Y -= height;

			toolbarShadow.Frame = frame;

			if (animate)
				UIView.CommitAnimations ();

			activeAssignmentVisible = visible;
		}

		/// <summary>
		/// Sets the active assignment's views
		/// </summary>
		void LoadActiveAssignment ()
		{
			var assignment = assignmentViewModel.ActiveAssignment;

			//Update font size on priority
			priority.Font = Theme.FontOfSize (assignment.Priority >= 10 ? 14f : 18f);

			priority.Text = assignment.Priority.ToString ();
			numberAndDate.Text = string.Format ("#{0} {1}", assignment.JobNumber, assignment.StartDate.Date.ToShortDateString ());
			titleLabel.Text = assignment.CompanyName;
			startAndEnd.Text = assignment.FormatStartEndDates ();
			contact.TopLabel.Text = assignment.ContactName;
			contact.BottomLabel.Text = assignment.ContactPhone;
			address.TopLabel.Text = assignment.Address;
			address.BottomLabel.Text = string.Format ("{0}, {1} {2}", assignment.City, assignment.State, assignment.Zip);
			status.Assignment = assignment;
		}

		/// <summary>
		/// Event when record button is pressed
		/// </summary>
		partial void Record ()
		{
			record.Enabled = false;
			Task task = assignmentViewModel.Recording ? assignmentViewModel.PauseAsync () : assignmentViewModel.RecordAsync ();
			task.ContinueWith (_ => BeginInvokeOnMainThread (() => record.Enabled = true));
		}

		/// <summary>
		/// Event when the address is clicked on the active assignment
		/// </summary>
		partial void Address ()
		{
			//Change to maps
			var menuViewModel = ServiceContainer.Resolve<MenuViewModel>();
			menuViewModel.MenuIndex = SectionIndex.Maps;
			assignmentViewModel.SelectedAssignment = assignmentViewModel.ActiveAssignment;
			PerformSegue ("AssignmentDetails", this);
		}

		/// <summary>
		/// Data source for the tableView of assignments
		/// </summary>
		class TableSource : UITableViewSource
		{
			readonly AssignmentsController controller;
			readonly AssignmentViewModel assignmentViewModel;
				
			public TableSource (AssignmentsController controller)
			{
				this.controller = controller;
				assignmentViewModel = ServiceContainer.Resolve<AssignmentViewModel>();
			}

			public override nint RowsInSection (UITableView tableView, nint section)
			{
				return assignmentViewModel.Assignments == null ? 0 : assignmentViewModel.Assignments.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var assignment = assignmentViewModel.Assignments [indexPath.Row];
				var cell = tableView.DequeueReusableCell ("AssignmentCell") as AssignmentCell;
				cell.SetAssignment (controller, assignment, indexPath);
				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				var menuViewModel = ServiceContainer.Resolve<MenuViewModel>();
				menuViewModel.MenuIndex = SectionIndex.Summary;
				assignmentViewModel.SelectedAssignment = assignmentViewModel.Assignments[indexPath.Row];
				controller.PerformSegue ("AssignmentDetails", controller);
			}
		}
	}
}
