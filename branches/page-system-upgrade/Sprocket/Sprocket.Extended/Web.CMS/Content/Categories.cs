using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Sprocket.Utility;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class CategorySet : IPropertyEvaluatorExpression
	{
		private string name, label, hint;
		private bool allowMultiple, required;
		private List<Category> categories;
		private Category defaultCategory;

		public Category DefaultCategory
		{
			get { return defaultCategory; }
			set { defaultCategory = value; }
		}

		public List<Category> Categories
		{
			get { return categories; }
		}

		public bool AllowMultiple
		{
			get { return allowMultiple; }
			set { allowMultiple = value; }
		}

		public bool Required
		{
			get { return required; }
			set { required = value; }
		}

		public string Hint
		{
			get { return hint; }
			set { hint = value; }
		}

		public string Label
		{
			get { return label; }
			set { label = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public CategorySet(XmlElement xml)
		{
			name = xml.GetAttribute("Name");
			label = InnerText(xml, "Label");
			hint = InnerText(xml, "Hint");
			categories = new List<Category>();
			foreach (XmlElement x in xml.SelectNodes("Category"))
			{
				Category cat = new Category(x);
				if (cat.IsDefault)
					defaultCategory = cat;
				categories.Add(cat);
			}
			allowMultiple = StringUtilities.BoolFromString(xml.GetAttribute("AllowMultiple"));
			required = StringUtilities.BoolFromString(xml.GetAttribute("Required"));
		}

		private string InnerText(XmlElement xml, string xpath)
		{
			XmlElement x = xml.SelectSingleNode(xpath) as XmlElement;
			if (x == null) return "";
			return x.InnerText;
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "name":
				case "label":
				case "hint":
				case "allowmultiple":
				case "required":
				case "categories":
				case "defaultcategory":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "name": return name;
				case "label": return label;
				case "hint": return hint;
				case "allowmultiple": return allowMultiple;
				case "required": return required;
				case "categories": return categories;
				case "defaultcategory": return defaultCategory;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property for CategorySet.", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return "[CategorySet: " + name + "]";
		}
	}

	public class Category : IPropertyEvaluatorExpression
	{
		private string text;
		private bool isDefault;

		public bool IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
		}

		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		public Category(XmlElement xml)
		{
			isDefault = StringUtilities.BoolFromString(xml.GetAttribute("IsDefault"));
			text = xml.InnerText;
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "text":
				case "isdefault":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "text": return text;
				case "isdefault": return IsDefault;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property for CategorySet.", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return text;
		}
	}
}