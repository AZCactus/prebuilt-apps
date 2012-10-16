﻿//
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
using System.Text;
using System.Threading.Tasks;
using FieldService.Data;
using FieldService.Utilities;

namespace FieldService.ViewModels {
    /// <summary>
    /// View model for assignments list
    /// </summary>
    public class AssignmentViewModel : ViewModelBase {
        readonly IAssignmentService service;
        List<Assignment> assignments;
        Assignment activeAssignment;

        public AssignmentViewModel ()
        {
            service = ServiceContainer.Resolve<IAssignmentService> ();
        }

        /// <summary>
        /// The current active assignment, this can be null
        /// </summary>
        public Assignment ActiveAssignment
        {
            get { return activeAssignment; }
            set { activeAssignment = value; OnPropertyChanged ("ActiveAssignment"); }
        }

        /// <summary>
        /// List of assignments
        /// </summary>
        public List<Assignment> Assignments
        {
            get { return assignments; }
            set { assignments = value; OnPropertyChanged ("Assignments"); }
        }

        /// <summary>
        /// Loads the assignments asynchronously
        /// </summary>
        public Task LoadAssignmentsAsync ()
        {
            IsBusy = true;
            return service
                .GetAssignmentsAsync ()
                .ContinueOnUIThread (t => {
                    //Grab the active assignment
                    ActiveAssignment = t.Result.FirstOrDefault (a => a.Status == AssignmentStatus.Active);
                    //Grab everything besides the active assignment
                    Assignments = t.Result.Where (a => a.Status != AssignmentStatus.Active).ToList ();
                    IsBusy = false;
                    return t.Result;
                });
        }

        public Task SaveAssignment (Assignment assignment)
        {
            IsBusy = true;

            //Save the assignment
            var task = service.SaveAssignment (assignment);

            //If the active assignment
            if (activeAssignment != null &&
                assignment != activeAssignment &&
                assignment.Status == AssignmentStatus.Active) {

                //Set the active assignment to hold
                activeAssignment.Status = AssignmentStatus.Hold;
                task = task.ContinueWith (t => {
                    service.SaveAssignment (activeAssignment).Wait ();
                    return t.Result;
                });
            }

            //Attach a task to set IsBusy
            return task.ContinueOnUIThread (t => {
                IsBusy = false;
                return t.Result;
            });
        }
    }
}