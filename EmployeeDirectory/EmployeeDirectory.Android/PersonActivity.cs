
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using EmployeeDirectory.Data;
using EmployeeDirectory.ViewModels;
using System.IO;
using System.ComponentModel;

namespace EmployeeDirectory.Android
{
	[Activity (Label = "Person", Theme = "@android:style/Theme.Holo.Light")]			
	public class PersonActivity : ListActivity
	{
		PersonViewModel viewModel;

		IMenuItem favoriteItem;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			//
			// Get the person object from the intent
			//
			Person person;
			if (Intent.HasExtra ("Person")) {
				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
				var personBytes = Intent.GetByteArrayExtra ("Person");
				person = (Person)formatter.Deserialize (new MemoryStream (personBytes));
			}
			else {
				person = new Person ();
			}

			//
			// Load the View Model
			//
			viewModel = new PersonViewModel (person, MainActivity.SharedFavoritesRepository);
			viewModel.PropertyChanged += HandleViewModelPropertyChanged;

			//
			// Setup the UI
			//
			ListView.Divider = null;
			ListAdapter = new PersonAdapter (viewModel);

			Title = person.SafeDisplayName;
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsFavorite") {
				UpdateFavoriteIcon ();
			}
		}

		void UpdateFavoriteIcon ()
		{
			if (favoriteItem != null) {
				favoriteItem.SetIcon (
					viewModel.IsFavorite ?
					Resource.Drawable.btn_rating_star_on_normal_holo_light :
					Resource.Drawable.btn_rating_star_off_normal_holo_light);
			}
		}

		/// <summary>
		/// Creates the intent that can be used to present this activity given
		/// a specific Person object.
		/// </summary>
		/// <returns>
		/// The intent.
		/// </returns>
		/// <param name='person'>
		/// The Person to show in this activity.
		/// </param>
		public static Intent CreateIntent (Context context, Person person)
		{
			var intent = new Intent (context, typeof (PersonActivity));
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
			var personStream = new MemoryStream ();
			formatter.Serialize (personStream, person);
			intent.PutExtra ("Person", personStream.ToArray ());
			return intent;
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.PersonActivityOptionsMenu, menu);
			favoriteItem = menu.FindItem (Resource.Id.MenuFavorite);
			UpdateFavoriteIcon ();
			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.MenuFavorite) {
				viewModel.ToggleFavorite ();
				return true;
			}
			else {
				return base.OnOptionsItemSelected (item);
			}
		}
	}
}

