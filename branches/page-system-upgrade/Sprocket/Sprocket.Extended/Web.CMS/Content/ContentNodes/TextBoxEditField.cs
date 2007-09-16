using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using System.Web;

namespace Sprocket.Web.CMS.Content
{
	public class TextBoxEditField : IEditFieldHandler
	{
		public class TextBoxContentNodeCreator : IEditFieldObjectCreator
		{
			public string Identifier { get { return "TextBox"; } }
			public IEditFieldHandler CreateHandler() { return new TextBoxEditField(); }
			public IEditFieldHandlerDatabaseInterface CreateDatabaseInterface() { return new TextBoxNodeDatabaseInterface(); }
			public IEditFieldData CreateDataObject() { return new TextBoxData(); }
		}
		public class TextBoxNodeDatabaseInterface : IEditFieldHandlerDatabaseInterface
		{
			public void LoadDataList(List<EditFieldInfo> fields)
			{
				ContentManager.Instance.DataProvider.LoadDataList_TextBox(fields);
			}

			public Result SaveData(long dataID, IEditFieldData data)
			{
				return ContentManager.Instance.DataProvider.StoreEditField_TextBox(dataID, ((TextBoxData)data).Text);
			}
		}
		public class TextBoxData : IEditFieldData
		{
			private string text = String.Empty;
			public string Text
			{
				get { return text; }
				set { text = value; }
			}

			public override bool Equals(object obj)
			{
				if(!(obj is TextBoxData)) return false;
				return text.Equals(((TextBoxData)obj).Text);
			}

			public override int GetHashCode()
			{
				return text.GetHashCode();
			}

			public override string ToString()
			{
				return text;
			}
		}

		private string size = "30", maxLength = "", fieldName = "", style = "", classname = "text";

		public void Initialise(XmlElement xml)
		{
			if (xml.HasAttribute("Size"))
				size = xml.GetAttribute("Size");
			if (xml.HasAttribute("MaxLength"))
				maxLength = xml.GetAttribute("MaxLength");
			if (xml.HasAttribute("FieldName"))
				fieldName = xml.GetAttribute("FieldName");
			if (xml.HasAttribute("Class"))
				classname = xml.GetAttribute("Class");
			if (xml.HasAttribute("Style"))
				style = xml.GetAttribute("Style");
		}

		public string TypeName
		{
			get { return "TextBox"; }
		}

		public string FieldName
		{
			get { return fieldName; }
		}

		public void InitialiseData(IEditFieldData data)
		{
			((TextBoxData)data).Text = String.Empty;
		}

		public string RenderContent(IEditFieldData data)
		{
			return data == null ? "" : data.ToString();
		}

		public string RenderAdminField(IEditFieldData data)
		{
			return string.Format("<input type=\"text\" name=\"{0}\" class=\"{1}\" style=\"{2}\" value=\"{3}\" size=\"{4}\" maxlength=\"{5}\" />",
				fieldName, classname, style, HttpUtility.HtmlEncode(((TextBoxData)data).Text), size, maxLength);
		}

		public Result ReadAdminField(out IEditFieldData data)
		{
			TextBoxData box = new TextBoxData();
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
