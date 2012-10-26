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

using System.IO;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using FieldService.Data;
using FieldService.Utilities;
using FieldService.ViewModels;

namespace FieldService.Android.Dialogs {
    /// <summary>
    /// Dialog for the photos
    /// </summary>
    public class PhotoDialog : BaseDialog, View.IOnClickListener, IDialogInterfaceOnClickListener {
        PhotoViewModel photoViewModel;
        ImageView photo;
        EditText optionalCaption;
        TextView photoCount,
            dateTime;
        public PhotoDialog (Context context)
            : base (context)
        {
            photoViewModel = ServiceContainer.Resolve<PhotoViewModel> ();
        }

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);
            SetContentView (Resource.Layout.AddPhotoPopUpLayout);

            optionalCaption = (EditText)FindViewById (Resource.Id.photoPopupDescription);
            dateTime = (TextView)FindViewById (Resource.Id.photoDateTime);
            photoCount = (TextView)FindViewById (Resource.Id.photoCountText);
            var delete = (Button)FindViewById (Resource.Id.photoDeleteImage);
            var nextPhoto = (Button)FindViewById (Resource.Id.photoNextButton);
            var previousPhoto = (Button)FindViewById (Resource.Id.photoPreviousButton);
            var cancel = (Button)FindViewById (Resource.Id.photoCancelImage);
            var done = (Button)FindViewById (Resource.Id.photoDoneImage);
            photo = (ImageView)FindViewById (Resource.Id.photoImageSource);

            nextPhoto.Visibility =
                previousPhoto.Visibility =
                photoCount.Visibility = ViewStates.Invisible;
            cancel.SetOnClickListener (this);
            done.SetOnClickListener (this);
            delete.SetOnClickListener (this);
        }

        /// <summary>
        /// Load the photo and description
        /// </summary>
        public override void OnAttachedToWindow ()
        {
            base.OnAttachedToWindow ();
            if (Photo != null) {
                dateTime.Text = string.Format ("{0} {1}", Photo.Date.ToString ("t"), Photo.Date.ToString ("d"));
                optionalCaption.Text = Photo.Description;
                using (var bmp = BitmapFactory.DecodeByteArray(Photo.Image, 0, Photo.Image.Length)) {
                    photo.SetImageBitmap(bmp);                    
                }
            } else if (PhotoStream != null) {
                using (var bmp = BitmapFactory.DecodeStream(PhotoStream)) {
                    photo.SetImageBitmap(bmp);                    
                }
            }
        }

        /// <summary>
        /// The parent activity
        /// </summary>
        public Activity Activity
        {
            get;
            set;
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
        /// The photo
        /// </summary>
        public Photo Photo
        {
            get;
            set;
        }

        /// <summary>
        /// Stream passed in from MediaPicker
        /// </summary>
        public Stream PhotoStream
        {
            get;
            set;
        }

        /// <summary>
        /// Show a dialog to delete the photo
        /// </summary>
        private void DeletePhoto ()
        {
            AlertDialog.Builder deleteDialog = new AlertDialog.Builder (Context);
            deleteDialog
                .SetTitle ("Delete?")
                .SetMessage ("Are you sure?")
                .SetPositiveButton ("Yes", this)
                .SetNegativeButton ("No", this)
                .Show();
        }

        /// <summary>
        /// Save the photo
        /// </summary>
        private void SavePhoto ()
        {
            Photo savePhoto = Photo;
            if (savePhoto == null) {
                savePhoto = new Photo ();
                try {
                    photo.DrawingCacheEnabled = true;
                    using (var bmp = photo.DrawingCache) {
                        if (bmp != null) {
                            using (MemoryStream stream = new MemoryStream ()) {
                                if (bmp.Compress (Bitmap.CompressFormat.Jpeg, 80, stream)) {
                                    savePhoto.Image = stream.ToArray ();
                                }
                            }
                        }
                    }
                } finally {
                    photo.DrawingCacheEnabled = false;
                }
            }
            savePhoto.Description = optionalCaption.Text;
            savePhoto.Assignment = Assignment.ID;

            photoViewModel.SavePhoto (Assignment, savePhoto)
                .ContinueOnUIThread (_ => {
                    ((SummaryActivity)Activity).ReloadConfirmation ();
                    Dismiss ();
                });
        }

        /// <summary>
        /// Click handlers
        /// </summary>
        public void OnClick (View v)
        {
            switch (v.Id) {
                case Resource.Id.photoDeleteImage:
                    //delete current photo
                    DeletePhoto ();
                    break;
                case Resource.Id.photoDoneImage:
                    //save photo
                    SavePhoto ();
                    break;
                case Resource.Id.photoCancelImage:
                    Dismiss ();
                    break;
                case Resource.Id.photoNextButton:
                    //go to next photo
                    break;
                case Resource.Id.photoPreviousButton:
                    //go to previous photo
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// When "Yes" is clicked to delete the photo
        /// </summary>
        public void OnClick (IDialogInterface dialog, int which)
        {
            if (which == 0) {
                photoViewModel
                    .DeletePhoto (Assignment, Photo)
                    .ContinueOnUIThread (_ => {
                        ((SummaryActivity)Activity).ReloadConfirmation ();
                        Dismiss ();
                    });
            }
            dialog.Dismiss ();
        }
    }
}