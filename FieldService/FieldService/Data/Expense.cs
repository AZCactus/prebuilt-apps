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

using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FieldService.Utilities;

namespace FieldService.Data {
    /// <summary>
    /// An expense associated with an assignment
    /// </summary>
    public class Expense {
        /// <summary>
        /// ID of the item
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        /// <summary>
        /// A category to identify the expense
        /// </summary>
        public ExpenseCategory Category { get; set; }

        /// <summary>
        /// An extended description of the expense
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The cost of the expense
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// An attached photo
        /// </summary>
        public Byte[] Photo { get; set; }

        /// <summary>
        /// Link to an assignment
        /// </summary>
        [Indexed]
        public int Assignment { get; set; }

        public string TypeAsString
        {
            get
            {
                return Category.ToUserString ();
            }
        }
    }
}