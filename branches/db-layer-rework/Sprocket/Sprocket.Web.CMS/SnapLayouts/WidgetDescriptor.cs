using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web.CMS.SnapLayouts
{
	public class WidgetDescriptor : IJSONEncoder
	{
		private string title = "", description = "", imageURL = "", widgetTypeID = "";

		public WidgetDescriptor(string widgetTypeID)
		{
			this.widgetTypeID = widgetTypeID;
		}

		public string WidgetTypeID
		{
			get { return widgetTypeID; }
		}

		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		public string ImageURL
		{
			get { return imageURL; }
			set { imageURL = value; }
		}

		public static string DefaultImagePath
		{
			get { return "datastore/snaplayouts/defaultWidgetIcon.gif"; }
		}

		public void WriteJSON(System.IO.StringWriter writer)
		{
			JSON.EncodeCustomObject(writer,
				"WidgetTypeID", widgetTypeID,
				"Title", title,
				"Description", description,
				"ImageURL", imageURL
			);

		}
	}
}
