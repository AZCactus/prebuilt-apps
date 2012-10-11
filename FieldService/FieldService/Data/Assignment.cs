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

namespace FieldService.Data
{
    /// <summary>
    /// An assignment is the "thing" or "job" the user is going to work on
    /// </summary>
    public class Assignment
    {
        /// <summary>
        /// The assignment's status
        /// </summary>
        public AssignmentStatus Status { get; set; }

        /// <summary>
        /// A job number
        /// </summary>
        public string JobNumber { get; set; }

        public string Title { get; set; }
        
        /// <summary>
        /// Date & time the assignment should start
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date & time the assignment should end
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Name of the contact
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// Phone number for the contact
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Address for assignment
        /// </summary>
        public string Address { get; set; }
    }
}
