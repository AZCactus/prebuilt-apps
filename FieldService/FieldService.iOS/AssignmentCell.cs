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
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FieldService.Data;

namespace FieldService.iOS
{
	public partial class AssignmentCell : UITableViewCell
	{
		public AssignmentCell (IntPtr handle) : base (handle)
		{
			SelectedBackgroundView = new UIView { BackgroundColor = Theme.AssignmentBlue };
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			priority.TextColor = UIColor.White;
			priorityBackground.Image = Theme.NumberBox;

			numberAndDate.HighlightedTextColor =
				title.HighlightedTextColor =
				startAndEnd.HighlightedTextColor = Theme.LabelColor;
		}

		public void SetAssignment (Assignment assignment)
		{
			BackgroundColor = assignment.Status == AssignmentStatus.Enroute ? Theme.AssignmentBlue : Theme.AssignmentGrey;
			priority.Text = assignment.Priority.ToString ();
			numberAndDate.Text = string.Format ("#{0} {1}", assignment.JobNumber, assignment.StartDate.Date.ToShortDateString ());
			title.Text = assignment.Title;
			startAndEnd.Text = string.Format ("Start: {0} End: {1}", assignment.StartDate.ToShortTimeString (), assignment.EndDate.ToShortTimeString ());
		}

		protected override void Dispose (bool disposing)
		{
			ReleaseDesignerOutlets ();

			base.Dispose (disposing);
		}
	}
}
