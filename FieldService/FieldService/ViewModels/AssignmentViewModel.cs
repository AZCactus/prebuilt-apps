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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldService.Data;
using FieldService.Utilities;

#if NETFX_CORE
using Timer = FieldService.WinRT.Utilities.Timer;
#else
using System.Timers;
#endif

namespace FieldService.ViewModels {
    /// <summary>
    /// View model for assignments list
    /// </summary>
    public class AssignmentViewModel : ViewModelBase {
        readonly IAssignmentService service;
        readonly Timer timer;
        TimeSpan hours = TimeSpan.Zero;
        List<Assignment> assignments;
        Assignment activeAssignment;
        TimerEntry timerEntry;

        /// <summary>
        /// Event when Hours is updated
        /// </summary>
        public event EventHandler HoursChanged;

        /// <summary>
        /// Event when Recording is changed
        /// </summary>
        public event EventHandler RecordingChanged;

        public AssignmentViewModel ()
        {
            service = ServiceContainer.Resolve<IAssignmentService> ();

            timer = new Timer (1000);

#if !NETFX_CORE
            //This causes our timer to fire it's events on the UI thread
            timer.SynchronizingObject = ServiceContainer.Resolve<ISynchronizeInvoke> ();
#endif
            timer.Elapsed += (sender, e) => {
                Hours = hours.Add (TimeSpan.FromSeconds (1));
            };
        }

        /// <summary>
        /// True if the active assignment is recording hours
        /// </summary>
        public bool Recording
        {
            get { return timer.Enabled; }
            set { timer.Enabled = value; OnRecordingChanged (); }
        }

        /// <summary>
        /// Called when Recording changes
        /// </summary>
        protected virtual void OnRecordingChanged ()
        {
            OnPropertyChanged ("Recording");
            var method = RecordingChanged;
            if (method != null)
                RecordingChanged (this, EventArgs.Empty);
        }

        /// <summary>
        /// Number of accumulated hours on the active assignment
        /// </summary>
        public TimeSpan Hours
        {
            get { return hours; }
            set { hours = value; OnHoursChanged (); }
        }

        /// <summary>
        /// Called when Hours changes
        /// </summary>
        protected virtual void OnHoursChanged ()
        {
            OnPropertyChanged ("Hours");

            var method = HoursChanged;
            if (method != null)
                HoursChanged (this, EventArgs.Empty);
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
        public Task LoadAssignments ()
        {
            return service
                .GetAssignmentsAsync ()
                .ContinueOnUIThread (t => {
                    //Grab the active assignment
                    ActiveAssignment = t.Result.FirstOrDefault (a => a.Status == AssignmentStatus.Active);
                    //Grab everything besides the active assignment
                    Assignments = t.Result.Where (a => a.Status != AssignmentStatus.Active).ToList ();
                    return t.Result;
                });
        }

        /// <summary>
        /// Loads the timer entry
        /// </summary>
        public Task LoadTimerEntry ()
        {
            return service
                .GetTimerEntryAsync ()
                .ContinueOnUIThread (t => {
                    timerEntry = t.Result;
                    if (timerEntry != null) {
                        Hours = (DateTime.Now - timerEntry.Date);
                        timer.Enabled = true;
                    }
                    return timerEntry;
                });
        }

        /// <summary>
        /// Saves an assignment, and applies any needed changes to the active one
        /// </summary>
        public Task SaveAssignment (Assignment assignment)
        {
            //Save the assignment
            Task task = service.SaveAssignment (assignment);

            //If the active assignment should be put on hold
            if (activeAssignment != null &&
                assignment != activeAssignment &&
                assignment.Status == AssignmentStatus.Active) {

                //Set the active assignment to hold and save it
                activeAssignment.Status = AssignmentStatus.Hold;
                if (Recording) {
                    task = task.ContinueWith (Pause ());
                }
                task = task.ContinueWith (service.SaveAssignment (activeAssignment));
            }

            //If we are saving the active assignment, we need to pause it
            if (assignment == activeAssignment &&
                assignment.Status != AssignmentStatus.Active &&
                Recording) {
                task = task.ContinueWith (Pause ());
            }

            //Set the active assignment
            if (assignment.Status == AssignmentStatus.Active) {
                ActiveAssignment = assignment;
            }

            return task;
        }

        /// <summary>
        /// Starts timer
        /// </summary>
        public Task Record ()
        {
            if (activeAssignment == null)
                return Task.Factory.StartNew (delegate { });

            Recording = true;

            if (timerEntry == null)
                timerEntry = new TimerEntry ();
            timerEntry.Date = DateTime.Now;

            return service.SaveTimerEntry (timerEntry);
        }

        /// <summary>
        /// Pauses timer
        /// </summary>
        public Task Pause ()
        {
            if (activeAssignment == null)
                return Task.Factory.StartNew (delegate { });

            Recording = false;

            var labor = new Labor {
                Type = LaborType.Hourly,
                Assignment = activeAssignment.ID,
		Description = "Time entered automatically at: " + DateTime.Now.ToShortTimeString (),
                Hours = (DateTime.Now - timerEntry.Date),
            };

            return service
                .SaveLabor (labor)
                .ContinueWith (service.DeleteTimerEntry (timerEntry))
                .ContinueOnUIThread (_ => Hours = TimeSpan.Zero);
        }
    }
}