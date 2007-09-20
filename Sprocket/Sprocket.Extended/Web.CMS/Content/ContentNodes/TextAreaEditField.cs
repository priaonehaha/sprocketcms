using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using System.Web;

namespace Sprocket.Web.CMS.Content
{
	public class TextAreaEditField : IEditFieldHandler
	{
		public class TextAreaContentNodeCreator : IEditFieldObjectCreator
		{
			public string Identifier { get { return "TextArea"; } }
			public IEditFieldHandler CreateHandler() { return new TextAreaEditField(); }
			public IEditFieldHandlerDatabaseInterface CreateDatabaseInterface() { return new TextAreaNodeDatabaseInterface(); }
			public IEditFieldData CreateDataObject() { return new TextBoxEditField.TextBoxData(); }
		}
		public class TextAreaNodeDatabaseInterface : IEditFieldHandlerDatabaseInterface
		{
			public void LoadDataList(List<EditFieldInfo> fields)
			{
				ContentManager.Instance.DataProvider.LoadDataList_TextBox(fields);
			}

			public Result SaveData(long dataID, IEditFieldData data)
			{
				return ContentManager.Instance.DataProvider.StoreEditField_TextBox(dataID, ((TextBoxEditField.TextBoxData)data).Text);
			}
		}

		private string size = "30", lines = "4", fieldName = "", style = "", classname = "text";

		public void Initialise(XmlElement xml)
		{
			if (xml.HasAttribute("Size"))
				size = xml.GetAttribute("Size");
			if (xml.HasAttribute("Lines"))
				lines = xml.GetAttribute("Lines");
			if (xml.HasAttribute("FieldName"))
				fieldName = xml.GetAttribute("FieldName");
			if (xml.HasAttribute("Class"))
				classname = xml.GetAttribute("Class");
			if (xml.HasAttribute("Style"))
				style = xml.GetAttribute("Style");
		}

		public string TypeName
		{
			get { return "TextArea"; }
		}

		public string FieldName
		{
			get { return fieldName; }
		}

		public void InitialiseData(IEditFieldData data)
		{
			((TextBoxEditField.TextBoxData)data).Text = String.Empty;
		}

		public object GetOutputValue(IEditFieldData data)
		{
			return data == null ? "" : data.ToString();
		}

		public string RenderAdminField(IEditFieldData data)
		{
			return string.Format("<textarea name=\"{0}\" class=\"{1}\" style=\"{2}\" cols=\"{4}\" rows=\"{5}\">{3}</textarea>",
				fieldName, classname, style, HttpUtility.HtmlEncode(((TextBoxEditField.TextBoxData)data).Text), size, lines);
		}

		public Result ReadAdminField(out IEditFieldData data)
		{
			TextBoxEditField.TextBoxData box = new TextBoxEditField.TextBoxData();
			string text = HttpContext.Current.Request.Form[fieldName];
			box.Text = text ?? "";
			data = box;
			return new Result();
		}

		public bool IsContentDifferent(IEditFieldData a, IEditFieldData b)
		{
			return !a.Equals(b);
		}
	}
}
