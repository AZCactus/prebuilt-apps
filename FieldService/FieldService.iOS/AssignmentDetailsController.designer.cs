// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace FieldService.iOS
{
	[Register ("AssignmentDetailsController")]
	partial class AssignmentDetailsController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel description { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel descriptionTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView priorityBackground { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel priority { get; set; }

		[Outlet]
		FieldService.iOS.TextButton contact { get; set; }

		[Outlet]
		FieldService.iOS.TextButton address { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel numberAndDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel title { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel startAndEnd { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView assignmentBackground { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView descriptionBackground { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIToolbar toolbar { get; set; }

		[Outlet]
		FieldService.iOS.StatusButton status { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton accept { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton decline { get; set; }

		[Action ("Accept")]
		partial void Accept ();

		[Action ("Decline")]
		partial void Decline ();
		
		void ReleaseDesignerOutlets ()
		{
			if (description != null) {
				description.Dispose ();
				description = null;
			}

			if (descriptionTitle != null) {
				descriptionTitle.Dispose ();
				descriptionTitle = null;
			}

			if (priorityBackground != null) {
				priorityBackground.Dispose ();
				priorityBackground = null;
			}

			if (priority != null) {
				priority.Dispose ();
				priority = null;
			}

			if (contact != null) {
				contact.Dispose ();
				contact = null;
			}

			if (address != null) {
				address.Dispose ();
				address = null;
			}

			if (numberAndDate != null) {
				numberAndDate.Dispose ();
				numberAndDate = null;
			}

			if (title != null) {
				title.Dispose ();
				title = null;
			}

			if (startAndEnd != null) {
				startAndEnd.Dispose ();
				startAndEnd = null;
			}

			if (assignmentBackground != null) {
				assignmentBackground.Dispose ();
				assignmentBackground = null;
			}

			if (descriptionBackground != null) {
				descriptionBackground.Dispose ();
				descriptionBackground = null;
			}

			if (toolbar != null) {
				toolbar.Dispose ();
				toolbar = null;
			}

			if (status != null) {
				status.Dispose ();
				status = null;
			}

			if (accept != null) {
				accept.Dispose ();
				accept = null;
			}

			if (decline != null) {
				decline.Dispose ();
				decline = null;
			}
		}
	}
}
