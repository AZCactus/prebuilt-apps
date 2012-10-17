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
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FieldService.Utilities;

namespace FieldService.Data
{
    /// <summary>
    /// A helper class for working with SQLite
    /// </summary>
    public static class Database
    {
#if NCRUNCH
        private static readonly string Path = System.IO.Path.Combine (Environment.CurrentDirectory, "Database.db");

        static Database()
        {
            Console.WriteLine("Database Path: " + Path);
        }
#elif NETFX_CORE
        private static readonly string Path = "Database.db"; //TODO: change this later
#else
        private static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Database.db");
#endif
        private static bool initialized = false;
        private static readonly Type [] tableTypes = new Type []
        {
            typeof(Assignment),
            typeof(Item),
            typeof(AssignmentItem),
            typeof(Labor),
            typeof(Expense),
            typeof(TimerEntry),
        };

        /// <summary>
        /// For use within the app on startup, this will create the database
        /// </summary>
        /// <returns></returns>
        public static Task Initialize()
        {
            return CreateDatabase(new SQLiteAsyncConnection(Path, true));
        }

        /// <summary>
        /// Global way to grab a connection to the database, make sure to wrap in a using
        /// </summary>
        public static SQLiteAsyncConnection GetConnection()
        {
            var connection = new SQLiteAsyncConnection(Path, true);
            if (!initialized)
            {
                CreateDatabase(connection).Wait();
            }
            return connection;
        }

        /// <summary>
        /// A helper method to reset the database
        /// </summary>
        public static Task DropTables ()
        {
            var connection = new SQLiteAsyncConnection (Path, true);
            return connection
                .DropTableAsync<Assignment> ()
                .ContinueWith (connection.DropTableAsync<Item> ())
                .ContinueWith (connection.DropTableAsync<AssignmentItem> ())
                .ContinueWith (connection.DropTableAsync<Labor> ())
                .ContinueWith (connection.DropTableAsync<Expense> ())
                .ContinueWith (connection.DropTableAsync<TimerEntry> ());
        }

        private static Task CreateDatabase(SQLiteAsyncConnection connection)
        {
            return Task.Factory.StartNew(() =>
            {
                //Create the tables
                var createTask = connection.CreateTablesAsync (tableTypes);
                createTask.Wait();

                //Count number of assignments
                var countTask = connection.Table<Assignment>().CountAsync();
                countTask.Wait();

                //If no assignments exist, insert our initial data
                if (countTask.Result == 0)
                {
                    var insertTask = connection.InsertAllAsync(new object[]
                    {
                        //Some assignments
                        new Assignment
                        {
                            ID = 1,
                            Priority = 1,
                            JobNumber = "1",
                            Title = "Assignment 1",
			    ContactName = "Miguel de Icaza",
			    ContactPhone = "1.232.234.2352",
			    Address = "306 5th Street",
			    City = "Adrian",
			    State = "TX",
			    Zip = "79001",
                            Status = AssignmentStatus.Active,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddHours(2),
                        },
                        new Assignment
                        {
                            ID = 2,
                            Priority = 2,
                            JobNumber = "2",
                            Title = "Assignment 2",
			    ContactName = "Greg Shackles",
			    ContactPhone = "1.232.234.2112",
			    Address = "503 Community Drive",
			    City = "New York",
			    State = "NY",
			    Zip = "11787",
                            Status = AssignmentStatus.Hold,
                            StartDate = DateTime.Now.AddDays(1),
                            EndDate = DateTime.Now.AddDays(1).AddHours(2),
                        },
			new Assignment
			{
			    ID = 3,
			    Priority = 3,
                            JobNumber = "3",
			    Title = "Assignment 3",
			    ContactName = "Xamarin",
			    ContactPhone = "1.855.926.2746",
			    Address = "1796 18th Street",
			    City = "San Fancisco",
			    State = "CA",
			    Zip = "94107",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
			},
                        new Assignment
			{
			    ID = 4,
			    Priority = 4,
                            JobNumber = "4",
			    Title = "Assignment 4",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "Bowling Green",
                            Zip = "42101",
                            State = "KY",
                            Address = "2425 Nashville Road",
                            ContactPhone = "270.796.5063",
                            ContactName = "HERB",
			},

                        new Assignment
			{
			    ID = 5,
			    Priority = 5,
                            JobNumber = "5",
			    Title = "Assignment 5",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "Mountain View",
                            Zip = "94043",
                            State = "CA",
                            Address = "1600 Amphitheatre Parkway",
                            ContactPhone = "650.253.0000",
                            ContactName = "Google",
			},

                        new Assignment
			{
			    ID = 6,
			    Priority = 6,
                            JobNumber = "6",
			    Title = "Assignment 6",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "Hershey",
                            Zip = "17033",
                            State = "PA",
                            Address = "200 Crystal A Drive",
                            ContactPhone = "717.298.1298",
                            ContactName = "Hershey",
			},

                        new Assignment
			{
			    ID = 7,
			    Priority = 7,
                            JobNumber = "7",
			    Title = "Assignment 7",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "Redmond",
                            Zip = "98052",
                            State = "WA",
                            Address = "1 157th Avenue Norhteast",
                            ContactPhone = "425.882.8080",
                            ContactName = "Microsoft",
			},

                        new Assignment
			{
			    ID = 8,
			    Priority = 8,
                            JobNumber = "8",
			    Title = "Assignment 8",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "New York",
                            Zip = "10118",
                            State = "NY",
                            Address = "350 5th Avenue",
                            ContactPhone = "212.736.3100",
                            ContactName = "The Empire",
			},

                        new Assignment
                        {
			    ID = 9,
			    Priority = 9,
                            JobNumber = "9",
			    Title = "Assignment 9",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "Chehalis",
                            Zip = "98532",
                            State = "WA",
                            Address = "465 Northwest Chehalis Avenue",
                            ContactPhone = "360.748.3720",
                            ContactName = "The Shire",
			},

                        new Assignment
                        {
			    ID = 10,
			    Priority = 10,
                            JobNumber = "10",
			    Title = "Assignment 10",
			    Status = AssignmentStatus.New,
			    StartDate = DateTime.Now.AddDays(1),
			    EndDate = DateTime.Now.AddDays(1).AddHours(2),
                            City = "Universal City",
                            Zip = "91608",
                            State = "CA",
                            Address = "100 Universal City Plaza",
                            ContactPhone = "800.864.8377",
                            ContactName = "Universal Studios",
			},

                        //Some items
                        new Item
                        {
                            ID = 1,
                            Name = "Sheet Metal Screws",
                            Number = "1001",
                        },
                        new Item
                        {
                            ID = 2,
                            Name = "PVC Pipe - Small",
                            Number = "1002",
                        },
                        new Item
                        {
                            ID = 3,
                            Name = "PVC Pipe - Medium",
                            Number = "1003",
                        },

                        //Some items on assignments
                        new AssignmentItem
                        {
                            Assignment = 1,
                            Item = 1,
                        },
                        new AssignmentItem
                        {
                            Assignment = 1,
                            Item = 2,
                        },
                        new AssignmentItem
                        {
                            Assignment = 2,
                            Item = 2,
                        },
                        new AssignmentItem
                        {
                            Assignment = 2,
                            Item = 3,
                        },
			new AssignmentItem
			{
			    Assignment = 3,
			    Item = 1,
			},
                        new AssignmentItem
			{
			    Assignment = 5,
			    Item = 1,
			},

                        //Some labor for assignments
                        new Labor
                        {
                            Assignment = 1,
                            Description = "Sheet Metal Screw Sorting",
                            Hours = TimeSpan.FromHours(10),
                            Type = LaborType.HolidayTime,
                        },
                        new Labor
                        {
                            Assignment = 1,
                            Description = "Pipe Fitting",
                            Hours = TimeSpan.FromHours(4),
                            Type = LaborType.Hourly,
                        },
                        new Labor
                        {
                            Assignment = 2,
                            Description = "Attaching Sheet Metal To PVC Pipe",
                            Hours = TimeSpan.FromHours(8),
                            Type = LaborType.OverTime,
                        },
                        new Labor
                        {
                            Assignment = 3,
                            Description = "Sheet Metal / Pipe Decoupling",
                            Hours = TimeSpan.FromHours(4),
                            Type = LaborType.Hourly,
                        },

                        //Some expenses for assignments
                        new Expense
                        {
                            Assignment = 1,
                            Description = "Filled up tank at Speedway",
                            Category = ExpenseCategory.Gas,
                            Cost = 40.5M,
                        },
                        new Expense
                        {
                            Assignment = 1,
                            Description = "Tasty Hot Dog from Speedway",
                            Category = ExpenseCategory.Food,
                            Cost = 0.99M,
                        },
                        new Expense
                        {
                            Assignment = 2,
                            Description = "Duct Tape",
                            Category = ExpenseCategory.Supplies,
                            Cost = 3.5M,
                        },
                        new Expense
                        {
                            Assignment = 3,
                            Description = "Passable Hot Dog from Speedway",
                            Category = ExpenseCategory.Food,
                            Cost = 0.99M,
                        },
                        new Expense
                        {
                            Assignment = 5,
                            Description = "Cost of Android being herp derp",
                            Category = ExpenseCategory.Other,
                            Cost = 255.5M,
                        },
                        new Expense
                        {
                            Assignment = 6,
                            Description = "Chocolate",
                            Category = ExpenseCategory.Food,
                            Cost = 1.5M,
                        },
                        new Expense
                        {
                            Assignment = 7,
                            Description = "Advertising",
                            Category = ExpenseCategory.Other,
                            Cost = 200M,
                        },
                        new Expense
                        {
                            Assignment = 8,
                            Description = "Cleaning Supplies",
                            Category = ExpenseCategory.Supplies,
                            Cost = .006M,
                        },
                        new Expense
                        {
                            Assignment = 9,
                            Description = "Ingredients",
                            Category = ExpenseCategory.Food,
                            Cost = .0001M,
                        },
                        new Expense
                        {
                            Assignment = 10,
                            Description = "Universal Stuff",
                            Category = ExpenseCategory.Supplies,
                            Cost = 9.1M,
                        },
                    });

                    //Wait for inserts
                    insertTask.Wait();

                    //Mark database created
                    initialized = true;
                }
            });
        }
    }
}