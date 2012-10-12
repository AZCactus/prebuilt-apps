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
using FieldService.ViewModels;
using FieldService.Data;
using FieldService.Utilities;

namespace FieldService.iOS
{
	public partial class AssignmentsController : UIViewController
	{
		readonly AssignmentViewModel assignmentViewModel;

		public AssignmentsController (IntPtr handle) : base (handle)
		{
			assignmentViewModel = new AssignmentViewModel (new SampleAssignmentService ());
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			tableView.DataSource = new DataSource (assignmentViewModel);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			assignmentViewModel.LoadAssignmentsAsync ().ContinueOnUIThread (_ => tableView.ReloadData ());
		}

		private class DataSource : UITableViewDataSource
		{
			readonly AssignmentViewModel assignmentViewModel;

			public DataSource (AssignmentViewModel assignmentViewModel)
			{
				this.assignmentViewModel = assignmentViewModel;
			}

			public override int RowsInSection (UITableView tableView, int section)
			{
				return assignmentViewModel.Assignments == null ? 0 : assignmentViewModel.Assignments.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var assignment = assignmentViewModel.Assignments [indexPath.Row];
				var cell = tableView.DequeueReusableCell ("AssignmentCell") as AssignmentCell;
				cell.SetAssignment (assignment);
				return cell;
			}
		}
	}
}
