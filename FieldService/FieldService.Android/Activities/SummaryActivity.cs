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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using FieldService.Android.Dialogs;
using FieldService.Android.Fragments;
using FieldService.Android.Utilities;
using FieldService.Data;
using FieldService.Utilities;
using FieldService.ViewModels;

namespace FieldService.Android {
    /// <summary>
    /// Activity for the summary screen
    /// </summary>
    [Activity (Label = "Summary", Theme = "@style/CustomHoloTheme")]
    public class SummaryActivity : Activity {
        readonly AssignmentViewModel assignmentViewModel;
        readonly ItemViewModel itemViewModel;
        readonly LaborViewModel laborViewModel;
        readonly PhotoViewModel photoViewModel;
        readonly ExpenseViewModel expenseViewModel;
        readonly DocumentViewModel documentViewModel;
        readonly HistoryViewModel historyViewModel;
        NavigationFragment navigationFragment;
        FrameLayout navigationFragmentContainer;
        TextView number, name, phone, address, items, title;
        Button addItems, addLabor, addExpense;
        ImageButton navigationMenu;
        int assignmentIndex = 0, navigationIndex = 0;
        ItemsDialog itemDialog;
        AddLaborDialog laborDialog;
        ExpenseDialog expenseDialog;
        LinearLayout mapButton,
            phoneButton;

        public SummaryActivity ()
        {
            assignmentViewModel = ServiceContainer.Resolve<AssignmentViewModel> ();
            itemViewModel = ServiceContainer.Resolve<ItemViewModel> ();
            laborViewModel = ServiceContainer.Resolve<LaborViewModel> ();
            photoViewModel = ServiceContainer.Resolve<PhotoViewModel> ();
            expenseViewModel = ServiceContainer.Resolve<ExpenseViewModel> ();
            documentViewModel = ServiceContainer.Resolve<DocumentViewModel> ();
            historyViewModel = ServiceContainer.Resolve<HistoryViewModel> ();
        }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.SummaryFragmentLayout);

            if (Intent != null) {
                assignmentIndex = Intent.GetIntExtra (Constants.BundleIndex, -1);
                if (assignmentIndex != -1) {
                    Assignment = assignmentViewModel.Assignments [assignmentIndex];
                } else {
                    Assignment = assignmentViewModel.ActiveAssignment;
                }
                navigationIndex = Intent.GetIntExtra (Constants.FragmentIndex, 0);
            }

            title = FindViewById<TextView> (Resource.Id.summaryAssignmentTitle);
            number = FindViewById<TextView> (Resource.Id.selectedAssignmentNumber);
            name = FindViewById<TextView> (Resource.Id.selectedAssignmentContactName);
            phone = FindViewById<TextView> (Resource.Id.selectedAssignmentPhoneNumber);
            address = FindViewById<TextView> (Resource.Id.selectedAssignmentAddress);
            items = FindViewById<TextView> (Resource.Id.selectedAssignmentTotalItems);
            addItems = FindViewById<Button> (Resource.Id.selectedAssignmentAddItem);
            addLabor = FindViewById<Button> (Resource.Id.selectedAssignmentAddLabor);
            addExpense = FindViewById<Button> (Resource.Id.selectedAssignmentAddExpense);
            navigationMenu = FindViewById<ImageButton> (Resource.Id.navigationMenu);
            navigationFragmentContainer = FindViewById<FrameLayout> (Resource.Id.navigationFragmentContainer);
            var back = FindViewById<LinearLayout> (Resource.Id.backSummary);
            mapButton = FindViewById<LinearLayout> (Resource.Id.summaryMapIconLayout);
            phoneButton = FindViewById<LinearLayout> (Resource.Id.summaryPhoneIconLayout);

            phoneButton.Click += (sender, e) => {
                Extensions.MakePhoneCall (this, phone.Text);
            };
            mapButton.Click += (sender, e) => {
                var navFragment = FragmentManager.FindFragmentById<NavigationFragment> (Resource.Id.navigationFragmentContainer);
                var index = Constants.Navigation.IndexOf("Map");
                navFragment.SetNavigation (index);
            };

            back.Click += (sender, e) => {
                OnBackPressed ();
            };

            if (Assignment != null) {
                title.Text = string.Format ("#{0} {1} {2}", Assignment.JobNumber, "Summary", Assignment.StartDate.ToShortDateString ());

                number.Text = Assignment.Priority.ToString ();
                name.Text = Assignment.ContactName;
                phone.Text = Assignment.ContactPhone;
                address.Text = string.Format ("{0}\n{1}, {2} {3}", Assignment.Address, Assignment.City, Assignment.State, Assignment.Zip);
            }

            //portrait mode, flip back and forth when selecting the navigation menu.
            if (navigationMenu != null) {
                navigationMenu.Click += (sender, e) => {
                    if (navigationFragmentContainer.Visibility == ViewStates.Visible) {
                        navigationFragmentContainer.Visibility = ViewStates.Invisible;
                    } else {
                        navigationFragmentContainer.Visibility = ViewStates.Visible;
                    }
                };
            } else {
                navigationFragmentContainer.Visibility = ViewStates.Visible;
            }

            //setting up default fragments
            var transaction = FragmentManager.BeginTransaction ();
            var summaryFragment = new SummaryFragment ();
            summaryFragment.Assignment = Assignment;
            navigationFragment = new NavigationFragment ();
            transaction.SetTransition (FragmentTransit.FragmentOpen);
            transaction.Add (Resource.Id.contentFrame, summaryFragment);
            transaction.Add (Resource.Id.navigationFragmentContainer, navigationFragment);
            transaction.Commit ();

            items.Visibility =
                 addItems.Visibility = ViewStates.Invisible;
            addLabor.Visibility = ViewStates.Gone;

            if (bundle != null && bundle.ContainsKey (Constants.BundleIndex)) {
                navigationIndex = bundle.GetInt (Constants.BundleIndex, 0);
            }
            addItems.Click += (sender, e) => {
                itemDialog = new ItemsDialog (this);
                itemDialog.Assignment = Assignment;
                itemDialog.Activity = this;
                itemDialog.Show ();
            };
            addLabor.Click += (sender, e) => {
                laborDialog = new AddLaborDialog (this);
                laborDialog.Assignment = Assignment;
                laborDialog.Activity = this;
                laborDialog.CurrentLabor = new Labor ();
                laborDialog.Show ();
            };
            addExpense.Click += (sender, e) => {
                //show add expense dialog;
                expenseDialog = new ExpenseDialog (this);
                expenseDialog.Assignment = Assignment;
                expenseDialog.Activity = this;
                expenseDialog.CurrentExpense = new Expense ();
                expenseDialog.Show ();
            };

            ServiceContainer.Register<SummaryActivity> (this);
        }

        protected override void OnSaveInstanceState (Bundle outState)
        {
            outState.PutInt (Constants.BundleIndex, navigationIndex);
            base.OnSaveInstanceState (outState);
        }

        private void NavigationSelected (object sender, EventArgs<int> e)
        {
            SetFrameFragment (e.Value);
            if (navigationMenu != null) {
                navigationFragmentContainer.Visibility = ViewStates.Invisible;
            }
            navigationIndex = e.Value;
            var screen = Constants.Navigation [e.Value];
            title.Text = string.Format ("#{0} {1} {2}", Assignment.JobNumber, screen, Assignment.StartDate.ToShortDateString ());
        }

        protected override void OnResume ()
        {
            base.OnResume ();
            if (navigationFragment != null) {
                navigationFragment.NavigationSelected += NavigationSelected;
            }
            if (navigationIndex != 0) {
                SetFrameFragment (navigationIndex);
            }
        }

        protected override void OnPause ()
        {
            base.OnPause ();
            if (navigationFragment != null) {
                navigationFragment.NavigationSelected -= NavigationSelected;
            }

            if (itemDialog != null) {
                if (itemDialog.IsShowing) {
                    itemDialog.Dismiss ();
                }
            }

            if (laborDialog != null) {
                if (laborDialog.IsShowing) {
                    laborDialog.Dismiss ();
                }
            }
        }

        /// <summary>
        /// The selected assignment
        /// </summary>
        public Assignment Assignment
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the child fragment for when each navigation item is selected
        /// </summary>
        private void SetFrameFragment (int index)
        {
            var transaction = FragmentManager.BeginTransaction ();
            var screen = Constants.Navigation [index];
            switch (screen) {
                case "Summary": {
                        var fragment = new SummaryFragment ();
                        fragment.Assignment = Assignment;
                        transaction.SetTransition (FragmentTransit.FragmentOpen);
                        transaction.Replace (Resource.Id.contentFrame, fragment);
                        transaction.Commit ();
                        items.Visibility =
                            addItems.Visibility = ViewStates.Invisible;
                        addExpense.Visibility =
                            addLabor.Visibility = ViewStates.Gone;
                    }
                    break;
                case "Map": {
                        var fragment = new MapFragment ();
                        fragment.AssignmentIndex = assignmentIndex;
                        transaction.SetTransition (FragmentTransit.FragmentOpen);
                        transaction.Replace (Resource.Id.contentFrame, fragment);
                        transaction.Commit ();
                        items.Visibility =
                            addItems.Visibility = ViewStates.Invisible;
                        addExpense.Visibility =
                            addLabor.Visibility = ViewStates.Gone;
                    }
                    break;
                case "Items": {
                        var fragment = new ItemFragment ();
                        fragment.Assignment = Assignment;
                        itemViewModel.LoadAssignmentItemsAsync (Assignment).ContinueOnUIThread (_ => {
                            fragment.AssignmentItems = itemViewModel.AssignmentItems;
                            transaction.SetTransition (FragmentTransit.FragmentOpen);
                            transaction.Replace (Resource.Id.contentFrame, fragment);
                            transaction.Commit ();
                            items.Visibility =
                                addItems.Visibility = ViewStates.Visible;
                            addExpense.Visibility =
                                addLabor.Visibility = ViewStates.Gone;
                            items.Text = string.Format ("({0}) Items", Assignment.TotalItems.ToString ());
                        });
                    }
                    break;
                case "Labor Hours": {
                        var fragment = new LaborHourFragment ();
                        laborViewModel.LoadLaborHoursAsync (Assignment).ContinueOnUIThread (_ => {
                            fragment.LaborHours = laborViewModel.LaborHours;
                            fragment.Assignment = Assignment;
                            transaction.SetTransition (FragmentTransit.FragmentOpen);
                            transaction.Replace (Resource.Id.contentFrame, fragment);
                            transaction.Commit ();
                            addLabor.Visibility =
                                items.Visibility = ViewStates.Visible;
                            addExpense.Visibility =
                                addItems.Visibility = ViewStates.Gone;
                            items.Text = string.Format ("{0} hrs", Assignment.TotalHours.TotalHours.ToString ("0.0"));
                        });
                    }
                    break;
                case "Confirmations": {
                        var fragment = new ConfirmationFragment ();
                        photoViewModel.LoadPhotosAsync (Assignment).ContinueOnUIThread (_ => {
                            fragment.Photos = photoViewModel.Photos;
                            fragment.Assignment = Assignment;
                            transaction.SetTransition (FragmentTransit.FragmentOpen);
                            transaction.Replace (Resource.Id.contentFrame, fragment);
                            transaction.Commit ();
                            addLabor.Visibility =
                                items.Visibility = ViewStates.Invisible;
                            addExpense.Visibility =
                                addItems.Visibility = ViewStates.Gone;
                        });
                    }
                    break;
                case "Expenses": {
                        var fragment = new ExpenseFragment ();
                        expenseViewModel.LoadExpensesAsync (Assignment).ContinueOnUIThread (_ => {
                            fragment.Expenses = expenseViewModel.Expenses;
                            fragment.Assignment = Assignment;
                            transaction.SetTransition (FragmentTransit.FragmentOpen);
                            transaction.Replace (Resource.Id.contentFrame, fragment);
                            transaction.Commit ();
                            addLabor.Visibility =
                                addItems.Visibility = ViewStates.Gone;
                            items.Visibility =
                                addExpense.Visibility = ViewStates.Visible;
                            items.Text = Assignment.TotalExpenses.ToString ("$0.00");
                        });
                    }
                    break;
                case "Documents": {
                        var fragment = new DocumentFragment ();
                        documentViewModel.LoadDocumentsAsync ().ContinueOnUIThread (_ => {
                            fragment.Documents = documentViewModel.Documents;
                            transaction.SetTransition (FragmentTransit.FragmentOpen);
                            transaction.Replace (Resource.Id.contentFrame, fragment);
                            transaction.Commit ();
                            items.Visibility =
                                addItems.Visibility = ViewStates.Invisible;
                            addExpense.Visibility =
                                addLabor.Visibility = ViewStates.Gone;
                            });
                    }
                    break;
                case "History": {
                        var fragment = new HistoryFragment ();
                        historyViewModel.LoadHistoryAsync (Assignment).ContinueOnUIThread (_ => {
                            fragment.History = historyViewModel.History;
                            transaction.SetTransition (FragmentTransit.FragmentOpen);
                            transaction.Replace (Resource.Id.contentFrame, fragment);
                            transaction.Commit ();
                            items.Visibility =
                                addItems.Visibility = ViewStates.Invisible;
                            addExpense.Visibility =
                                addLabor.Visibility = ViewStates.Gone;
                        });
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnBackPressed ()
        {
            Finish ();
        }
    }
}