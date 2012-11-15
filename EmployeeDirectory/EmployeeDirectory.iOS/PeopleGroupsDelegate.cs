//
//  Copyright 2012, Xamarin Inc.
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
//
using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using EmployeeDirectory.Data;

namespace EmployeeDirectory.iOS
{
	public class PeopleGroupsDelegate : UITableViewDelegate
	{
		UITableViewController controller;

		public event EventHandler<PersonSelectedEventArgs> PersonSelected;

		public PeopleGroupsDelegate (UITableViewController controller)
		{
			this.controller = controller;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var person = ((PeopleGroupsDataSource)tableView.DataSource).GetPerson (indexPath);

			var ev = PersonSelected;
			if (ev != null) {
				ev (this, new PersonSelectedEventArgs { Person = person });
			}
		}

		[Export ("scrollViewDidEndDragging:willDecelerate:")]
		public void DidEndDragging (UIScrollView scrollView, bool willDecelerate)
		{
			if (!willDecelerate) {
				var tableView = controller.TableView;
				((PeopleGroupsDataSource)tableView.DataSource).LoadImagesForOnscreenRows (tableView);
			}
		}

		[Export ("scrollViewDidEndDecelerating:")]
		public void DidEndDecelerating (UIScrollView scrollView)
		{
			var tableView = controller.TableView;
			((PeopleGroupsDataSource)tableView.DataSource).LoadImagesForOnscreenRows (tableView);
		}
	}

	public class PersonSelectedEventArgs : EventArgs
	{
		public Person Person { get; set; }
	}
}

