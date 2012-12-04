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
using System.Threading;
using System.Threading.Tasks;

namespace FieldService.Data {
    public class SampleAssignmentService : IAssignmentService {
        private List<Document> _documents;

        public Task<List<Assignment>> GetAssignmentsAsync (CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .QueryAsync<Assignment> (@"
                    select Assignment.*, 
                           (SELECT SUM(Labor.Ticks) FROM Labor WHERE Assignment.Id = Labor.AssignmentId) as TotalTicks,       
                           (SELECT COUNT(AssignmentItem.Id) FROM AssignmentItem WHERE Assignment.Id = AssignmentItem.AssignmentId) AS TotalItems,       
                           (SELECT SUM(Expense.Cost) FROM Expense WHERE Assignment.Id = Expense.AssignmentId) AS TotalExpenses
                    from Assignment
                    where Assignment.Status != ? and Assignment.Status !=?
                    order by Assignment.Status DESC, Assignment.Priority
                ", AssignmentStatus.Declined, AssignmentStatus.Complete);
        }

        public Task<List<Item>> GetItemsAsync (CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .Table<Item> ()
                .OrderBy (i => i.Name)
                .ToListAsync ();
        }

        public Task<List<AssignmentItem>> GetItemsForAssignmentAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .QueryAsync<AssignmentItem> (@"
                    select AssignmentItem.*, Item.Number, Item.Name
                    from AssignmentItem
                    inner join Item
                    on Item.Id = AssignmentItem.ItemId
                    where AssignmentItem.AssignmentId = ?
                    order by Item.Name",
                    assignment.Id);
        }

        public Task<List<Labor>> GetLaborForAssignmentAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .Table<Labor> ()
                .Where (l => l.AssignmentId == assignment.Id)
                .ToListAsync ();
        }

        public Task<List<Expense>> GetExpensesForAssignmentAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .QueryAsync<Expense> (@"
                    select Expense.*, case when ExpensePhoto.Id is null then 0 else 1 end as HasPhoto
                    from Expense
                    left outer join ExpensePhoto
                    on ExpensePhoto.ExpenseId = Expense.Id
                    where Expense.AssignmentId = ?", 
                    assignment.Id);
        }

        public Task<List<Photo>> GetPhotosForAssignmentAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .Table<Photo> ()
                .Where (p => p.AssignmentId == assignment.Id)
                .ToListAsync ();
        }

        public Task<int> SaveAssignmentAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .UpdateAsync (assignment);
        }

        public Task<int> SaveSignatureAsync (Signature signature, CancellationToken cancellationToken)
        {
            if (signature.Id == 0)
                return Database.GetConnection (cancellationToken).InsertAsync (signature);
            else
                return Database.GetConnection (cancellationToken).UpdateAsync (signature);
        }

        public Task<Signature> GetSignatureAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).Table<Signature> ().Where (s => s.AssignmentId == assignment.Id).FirstOrDefaultAsync ();
        }

        public Task<int> SaveAssignmentItemAsync (AssignmentItem assignmentItem, CancellationToken cancellationToken)
        {
            if (assignmentItem.Id == 0)
                return Database.GetConnection (cancellationToken).InsertAsync (assignmentItem);
            else
                return Database.GetConnection (cancellationToken).UpdateAsync (assignmentItem);
        }

        public Task<int> SaveLaborAsync (Labor labor, CancellationToken cancellationToken)
        {
            if (labor.Id == 0)
                return Database.GetConnection (cancellationToken).InsertAsync (labor);
            else
                return Database.GetConnection (cancellationToken).UpdateAsync (labor);
        }

        public Task<int> SaveExpenseAsync (Expense expense, CancellationToken cancellationToken)
        {
            if (expense.Id == 0)
                return Database.GetConnection (cancellationToken).InsertAsync (expense);
            else
                return Database.GetConnection (cancellationToken).UpdateAsync (expense);
        }

        public Task<int> SavePhotoAsync (Photo photo, CancellationToken cancellationToken)
        {
            if (photo.Id == 0)
                return Database.GetConnection (cancellationToken).InsertAsync (photo);
            else
                return Database.GetConnection (cancellationToken).UpdateAsync (photo);
        }

        public Task<int> DeleteAssignmentAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).DeleteAsync (assignment);
        }

        public Task<int> DeleteAssignmentItemAsync (AssignmentItem assignmentItem, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).DeleteAsync (assignmentItem);
        }

        public Task<int> DeleteLaborAsync (Labor labor, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).DeleteAsync (labor);
        }

        public Task<int> DeleteExpenseAsync (Expense expense, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).DeleteAsync (expense);
        }

        public Task<int> DeletePhotoAsync (Photo photo, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).DeleteAsync (photo);
        }

        public Task<int> SaveTimerEntryAsync (TimerEntry entry, CancellationToken cancellationToken)
        {
            //If the Id is zero, it's an insert, also set the Id to 1
            if (entry.Id == 0) {
                entry.Id = 1;
                return Database.GetConnection (cancellationToken).InsertAsync (entry);
            } else {
                return Database.GetConnection (cancellationToken).UpdateAsync (entry);
            }
        }

        public Task<int> DeleteTimerEntryAsync (TimerEntry entry, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).DeleteAsync (entry);
        }

        public Task<TimerEntry> GetTimerEntryAsync (CancellationToken cancellationToken)
        {
            //Just return the first row
            return Database.GetConnection (cancellationToken).FindAsync<TimerEntry> (_ => true);
        }

        public Task<List<Document>> GetDocumentsAsync (CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew (() => {
                if (_documents == null) {
                    _documents = new List<Document> {
                        new Document { Title = "Hello, MVC", Path = "Data/Hello_MVC.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Building Cross Platform Apps", Path = "Data/Building_Cross_Platform_Apps.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Intro to Web Services", Path = "Data/Intro_to_Web_Services.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Introduction to Mobile Development", Path = "Data/Introduction_to_Mobile_Development.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Hello, iPhone", Path = "Data/Hello_iPhone.pdf", Type = DocumentType.ServiceAgreement },
                        new Document { Title = "Hello, Mono for Android", Path = "Data/Hello_Mono_for_Android.pdf", Type = DocumentType.Specifications },
                    };
                }
                return _documents;
            }, cancellationToken);
        }

        public Task<List<AssignmentHistory>> GetAssignmentHistoryAsync (Assignment assignment, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken)
                .QueryAsync<AssignmentHistory> (@"
                    select AssignmentHistory.*, Assignment.JobNumber, Assignment.CompanyName, Assignment.ContactName, Assignment.ContactPhone, Assignment.Address, Assignment.City, Assignment.State, Assignment.Zip
                    from AssignmentHistory
                    left outer join Assignment
                    on Assignment.Id = AssignmentHistory.AssignmentId
                    where AssignmentHistory.AssignmentId = ?
                    order by AssignmentHistory.Date desc
                ", assignment.Id);
        }

        public Task<int> SaveExpensePhotoAsync (ExpensePhoto photo, CancellationToken cancellationToken)
        {
            if (photo.Id == 0)
                return Database.GetConnection (cancellationToken).InsertAsync (photo);
            else
                return Database.GetConnection (cancellationToken).UpdateAsync (photo);
        }

        public Task<ExpensePhoto> GetExpensePhotoAsync (Expense expense, CancellationToken cancellationToken)
        {
            return Database.GetConnection (cancellationToken).Table<ExpensePhoto> ().Where (p => p.ExpenseId == expense.Id).FirstOrDefaultAsync ();
        }
    }
}