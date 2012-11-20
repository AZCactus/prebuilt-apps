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

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using FieldService.Android.Fragments;
using FieldService.Android.Utilities;
using FieldService.Data;
using FieldService.Utilities;
using FieldService.ViewModels;
using Xamarin.Controls;

namespace FieldService.Android.Dialogs {
    /// <summary>
    /// Dialog for capturing a signature
    /// </summary>
    public class SignatureDialog : BaseDialog {
        AssignmentViewModel assignmentViewModel;
        SignatureView signatureView;

        public SignatureDialog (Context context)
            : base (context)
        {
            var tabActivity = ServiceContainer.Resolve<AssignmentTabActivity> ();
            assignmentViewModel = tabActivity.AssignmentViewModel;
        }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.AddSignatureLayout);

            var save = (Button)FindViewById (Resource.Id.signatureSaveButton);
            save.Click += (sender, e) => {
                if (signatureView.IsBlank) {
                    AlertDialog.Builder builder = new AlertDialog.Builder (Activity);
                    builder
                        .SetTitle (string.Empty)
                        .SetMessage ("No signature!")
                        .SetPositiveButton ("Ok", (innerSender, innere) => { })
                        .Show ();
                    return;
                }
                var signature = new Signature {
                    AssignmentId = Assignment.Id,
                    Image = signatureView.GetImage ().ToByteArray (),
                    Id = assignmentViewModel.Signature != null ? assignmentViewModel.Signature.Id : 0
                };
                assignmentViewModel.SaveSignatureAsync (signature)
                    .ContinueOnUIThread (_ => {
                        var fragment = Activity.FragmentManager.FindFragmentById<ConfirmationFragment> (Resource.Id.contentFrame);
                        fragment.ReloadConfirmation ();
                        Dismiss ();
                    });
            };

            var cancel = (Button)FindViewById (Resource.Id.signatureCancelButton);
            cancel.Click += (sender, e) => {
                Dismiss ();
            };

            signatureView = (SignatureView)FindViewById (Resource.Id.signatureImage);
        }

        /// <summary>
        /// The activity holding this dialog
        /// </summary>
        public Activity Activity
        {
            get;
            set;
        }

        /// <summary>
        /// The current assignment
        /// </summary>
        public Assignment Assignment
        {
            get;
            set;
        }
    }
}