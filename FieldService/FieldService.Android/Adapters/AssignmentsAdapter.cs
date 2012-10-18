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
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using FieldService.Data;
using FieldService.Utilities;
using FieldService.ViewModels;
using Orientation = Android.Content.Res.Orientation;

namespace FieldService.Android {
    public class AssignmentsAdapter : ArrayAdapter<Assignment>, View.IOnClickListener{

        AssignmentViewModel assignmentViewModel;
        IList<Assignment> assignments;
        List<string> assignmentStatus;
        int resourceId;

        public AssignmentsAdapter (Context context, int resourceId, IList<Assignment> assignments)
            : base (context, resourceId, assignments)
        {
            this.assignments = assignments;
            this.resourceId = resourceId;
            this.assignmentViewModel = ServiceContainer.Resolve<AssignmentViewModel> ();

            assignmentStatus = new List<string> ();
            foreach (var item in Assignment.AvailableStatuses) {
                assignmentStatus.Add (item.ToString ());
            }
        }

        public override View GetView (int position, View convertView, ViewGroup parent)
        {
            Assignment assignment = null;
            var view = convertView;
            if (view == null) {
                LayoutInflater inflator = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
                view = inflator.Inflate (resourceId, null);
            }

            if (assignments.Count > position) {
                assignment = assignments [position];
            }

            if (assignment == null) {
                return view;
            }

            var number = view.FindViewById<TextView> (Resource.Id.assignmentItemNumber);
            var job = view.FindViewById<TextView> (Resource.Id.assignmentJob);
            var name = view.FindViewById<TextView> (Resource.Id.assignmentName);
            var phone = view.FindViewById<TextView> (Resource.Id.assignmentPhone);
            var address = view.FindViewById<TextView> (Resource.Id.assignmentAddress);
            var buttonLayout = view.FindViewById<LinearLayout> (Resource.Id.assignmentButtonLayout);
            var timerLayout = view.FindViewById<LinearLayout> (Resource.Id.assignmentTimerLayout);
            var timerlinearLayout = view.FindViewById<LinearLayout> (Resource.Id.timerLinearLayout);
            var spinner = view.FindViewById<Spinner> (Resource.Id.assignmentStatus);
            var spinnerImage = view.FindViewById<ImageView> (Resource.Id.assignmentStatusImage);
            var timer = view.FindViewById<ToggleButton> (Resource.Id.assignmentTimer);
            var timerText = view.FindViewById<TextView> (Resource.Id.assignmentTimerText);
            var accept = view.FindViewById<Button> (Resource.Id.assignmentAccept);
            var decline = view.FindViewById<Button> (Resource.Id.assignmentDecline);

            if (assignment.Status == AssignmentStatus.New) {
                buttonLayout.Visibility = ViewStates.Visible;
                timerLayout.Visibility = ViewStates.Gone;
                accept.Tag = position;
                decline.Tag = position;
                view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentoffwhite));

            } else {
                buttonLayout.Visibility = ViewStates.Gone;
                timerLayout.Visibility = ViewStates.Visible;
                timerlinearLayout.Visibility = Context.Resources.Configuration.Orientation == Orientation.Landscape ? ViewStates.Invisible : ViewStates.Gone;

                spinner.Tag = position;
                var adapter = new SpinnerAdapter (assignmentStatus, ServiceContainer.Resolve<AssignmentsActivity> ());
                spinner.Adapter = adapter;

                spinner.SetSelection (assignmentStatus.IndexOf (assignment.Status.ToString ()));
                view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentgrey));
                spinnerImage.SetImageResource (Resource.Drawable.HoldImage);

                spinner.ItemSelected += (sender, e) => {
                    var selected = assignmentStatus.ElementAtOrDefault (e.Position);
                    var index = int.Parse (e.Parent.Tag.ToString ());
                    var activeAssignment = GetItem (index);
                    var status = (AssignmentStatus)Android.Utilities.Extensions.ToEnum (typeof (AssignmentStatus), selected);
                    if (activeAssignment.Status != status) {
                        switch (selected) {
                            case "Active": {
                                    view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentblue));
                                    spinnerImage.SetImageResource (Resource.Drawable.EnrouteImage);
                                    spinnerImage.InvalidateDrawable (spinnerImage.Drawable);
                                    spinner.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentblue));
                                    activeAssignment.Status = AssignmentStatus.Active;
                                    SaveAssignment (activeAssignment, index);
                                }
                                break;
                            default: {
                                    view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentgrey));
                                    spinnerImage.SetImageResource (Resource.Drawable.HoldImage);
                                    spinnerImage.InvalidateDrawable (spinnerImage.Drawable);
                                    spinner.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentgrey));
                                    activeAssignment.Status = status;
                                    SaveAssignment (activeAssignment, index);
                                }
                                break;
                        }
                    }
                };
            }

            number.Text = assignment.Priority.ToString ();
            job.Text = string.Format ("#{0} {1}\n{2}", assignment.JobNumber, assignment.StartDate.ToShortDateString (), assignment.Title);
            name.Text = assignment.ContactName;
            phone.Text = assignment.ContactPhone;
            address.Text = string.Format ("{0}\n{1}, {2} {3}", assignment.Address, assignment.City, assignment.State, assignment.Zip);
            accept.SetOnClickListener (this);
            decline.SetOnClickListener (this);

            return view;
        }

        /// <summary>
        /// Save assignment to the view model.
        /// </summary>
        private void SaveAssignment (Assignment assignment, int index)
        {
            assignmentViewModel.SaveAssignment (assignment).ContinueOnUIThread (_ => {
                var activity = ServiceContainer.Resolve<AssignmentsActivity> ();
                if (assignment.Status == AssignmentStatus.Active || assignment.Status == AssignmentStatus.Declined) {
                    activity.ReloadAssignments ();
                } else {
                    //not sure how to refresh 1 list item yet.
                    activity.ReloadSingleListItem (index);
                }
            });
        }

        public void OnClick (View v)
        {
            switch (v.Id) {
                case Resource.Id.assignmentAccept: {
                        var index = int.Parse (v.Tag.ToString ());
                        var activeAssignment = GetItem (index);
                        activeAssignment.Status = AssignmentStatus.Hold;
                        SaveAssignment (activeAssignment, index);
                    }
                    break;
                case Resource.Id.assignmentDecline: {
                        var index = int.Parse (v.Tag.ToString ());
                        var activeAssignment = GetItem (index);
                        activeAssignment.Status = AssignmentStatus.Declined;
                        SaveAssignment (activeAssignment, index);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}