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
using System.Text;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using FieldService.Data;
using FieldService.Utilities;

namespace FieldService.iOS
{
	/// <summary>
	/// Controller for displaying the map for an assignment
	/// </summary>
	public class MapController : BaseController
	{
		readonly AssignmentsController assignmentController;
		MKMapView mapView;

		public MapController ()
		{
			assignmentController = ServiceContainer.Resolve <AssignmentsController>();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//Setup the map view
			mapView = new MKMapView(View.Frame);
			mapView.ShowsUserLocation = true;
			mapView.AutoresizingMask = View.AutoresizingMask;
			mapView.Delegate = new MapViewDelegate();

			View.BackgroundColor = UIColor.Clear;
			View.AddSubview (mapView);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			//Load the placemark
			var placemark = assignmentController.Assignment.ToPlacemark();
			var span = new MKCoordinateSpan(1, 1);
			var region = new MKCoordinateRegion(placemark.Coordinate, span);

			mapView.ClearPlacemarks ();
			mapView.AddPlacemark (placemark);
			mapView.SetRegion (region, true);
		}

		/// <summary>
		/// Delegate for the map view
		/// </summary>
		private class MapViewDelegate : MKMapViewDelegate
		{
			const string Identifier = "AssignmentAnnotation";
			readonly UIPopoverController popoverController;

			public MapViewDelegate ()
			{
				popoverController = new UIPopoverController(new UIViewController());
				popoverController.PopoverContentSize = new System.Drawing.SizeF(100, 100);
			}

			/// <summary>
			/// Returns our custom MKAnnotationView
			/// </summary>
			public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
			{
				if (annotation is MKUserLocation) {
					return null;
				} else {
					var annotationView = mapView.DequeueReusableAnnotation (Identifier) as MKPinAnnotationView;
					if (annotationView == null) {
						annotationView = new MKPinAnnotationView(annotation, Identifier);
						annotationView.PinColor = MKPinAnnotationColor.Green;
						annotationView.AnimatesDrop = true;
						annotationView.CanShowCallout = true;
						annotationView.RightCalloutAccessoryView = UIButton.FromType (UIButtonType.DetailDisclosure);
					} else {
						annotationView.Annotation = annotation;
					}
					return annotationView;
				}
			}

			/// <summary>
			/// This is the callback for when the detail disclosure is clicked
			/// </summary>
			public override void CalloutAccessoryControlTapped (MKMapView mapView, MKAnnotationView view, UIControl control)
			{
				//This will launch apple's Maps app with the selected address
				var assignment = GetAssignment (view.Annotation as MKPlacemark);
				StringBuilder builder = new StringBuilder("http://maps.apple.com/maps?daddr=");
				builder.Append (assignment.Address.Replace (' ', '+'));
				builder.Append ('+');
				builder.Append (assignment.City.Replace (' ', '+'));
				builder.Append ('+');
				builder.Append (assignment.State);
				builder.Append ('+');
				builder.Append (assignment.Zip);
				builder.Append ('+');

				using (var url = NSUrl.FromString (builder.ToString ())) {
					UIApplication.SharedApplication.OpenUrl (url);
				}
			}

			/// <summary>
			/// This pulls out an assignment we placed in a MKPlacemark
			/// </summary>
			private Assignment GetAssignment(MKPlacemark annotation)
			{
				return annotation.AddressDictionary[new NSString("Assignment")].UnwrapObject<Assignment>();
			}
		}
	}
}

